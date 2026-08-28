using System;
using System.Collections.Generic;
using System.Linq;

namespace RestoJett.Core
{
    public interface IRestaurantService
    {
        // Meal operations
        Tuple<Exception, List<JMeal>> GetMeals(JUser loggedUser);
        Tuple<Exception, JMeal> AddMeal(JUser loggedUser, JMeal meal);
        Tuple<Exception, JMeal> UpdateMeal(JUser loggedUser, string mealGuid, JMeal meal);
        Tuple<Exception, bool> RemoveMeal(JUser loggedUser, string mealGuid);

        // User operations
        Tuple<Exception, List<JUser>> GetUsers(JUser loggedUser);
        Tuple<Exception, JUser> AddUser(JUser loggedUser, JUser user);
        Tuple<Exception, JUser> UpdateUser(JUser loggedUser, string userGuid, JUser user);
        Tuple<Exception, bool> RemoveUser(JUser loggedUser, string userGuid);

        // Customer operations
        Tuple<Exception, List<JCustomer>> GetCustomers(JUser loggedUser);
        Tuple<Exception, JCustomer> AddCustomer(JUser loggedUser, JCustomer customer);
        Tuple<Exception, JCustomer> UpdateCustomer(JUser loggedUser, string customerGuid, JCustomer customer);
        Tuple<Exception, bool> RemoveCustomer(JUser loggedUser, string customerGuid);

        // Order operations
        Tuple<Exception, List<JOrder>> GetOrders(JUser loggedUser);
        Tuple<Exception, JOrder> AddOrder(JUser loggedUser, JOrder order);
        Tuple<Exception, JOrder> UpdateOrder(JUser loggedUser, string orderGuid, JOrder order);
        Tuple<Exception, bool> RemoveOrder(JUser loggedUser, string orderGuid);

        // Authentication
        Tuple<Exception, JUser> Authenticate(string name, string password);

        // Audit logs (Admin only)
        Tuple<Exception, List<AuditLog>> GetAuditLogs(JUser loggedUser);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly List<JMeal> _meals;
        private readonly List<JUser> _users;
        private readonly List<JCustomer> _customers;
        private readonly List<JOrder> _orders;
        private readonly List<AuditLog> _auditLogs;
        private readonly object _lock = new object();

        public JMenu MainMenu { get; set; }

        public RestaurantService()
        {
            _meals = new List<JMeal>();
            _users = new List<JUser>();
            _customers = new List<JCustomer>();
            _orders = new List<JOrder>();
            _auditLogs = new List<AuditLog>();
            MainMenu = new JMenu();
        }

        #region Helper Methods

        private Tuple<Exception, bool> ValidateUser(JUser loggedUser, bool requireAdmin = false)
        {
            if (loggedUser == null)
            {
                return new Tuple<Exception, bool>(new UnauthorizedAccessException("User must be logged in to perform this operation."), false);
            }

            if (requireAdmin && loggedUser.UserType != JUserType.Admin)
            {
                return new Tuple<Exception, bool>(new UnauthorizedAccessException("Only administrators can perform this operation."), false);
            }

            return new Tuple<Exception, bool>(null, true);
        }

        private void LogAction(JUser loggedUser, string actionType, string entityType, string entityGuid, string details)
        {
            lock (_lock)
            {
                var auditLog = new AuditLog
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserGuid = loggedUser?.Guid ?? "anonymous",
                    UserName = loggedUser?.Name ?? "anonymous",
                    ActionType = actionType,
                    EntityType = entityType,
                    EntityGuid = entityGuid,
                    Details = details,
                    Timestamp = DateTime.Now
                };
                _auditLogs.Add(auditLog);
            }
        }

        private string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion

        #region Meal Operations

        public Tuple<Exception, List<JMeal>> GetMeals(JUser loggedUser)
        {
            var validation = ValidateUser(loggedUser);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, List<JMeal>>(validation.Item1, new List<JMeal>());
            }

            LogAction(loggedUser, "Read", "Meal", "*", "Retrieved all meals");
            return new Tuple<Exception, List<JMeal>>(null, _meals.ToList());
        }

        public Tuple<Exception, JMeal> AddMeal(JUser loggedUser, JMeal meal)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JMeal>(validation.Item1, null);
            }

            if (meal == null)
            {
                var ex = new ArgumentNullException(nameof(meal), "Meal cannot be null.");
                return new Tuple<Exception, JMeal>(ex, null);
            }

            meal.Guid = GenerateGuid();
            meal.Version = 1.0f;

            lock (_lock)
            {
                _meals.Add(meal);
                MainMenu.Meals[meal.Name] = meal;
            }

            LogAction(loggedUser, "Create", "Meal", meal.Guid, $"Added meal: {meal.Name}");
            return new Tuple<Exception, JMeal>(null, meal);
        }

        public Tuple<Exception, JMeal> UpdateMeal(JUser loggedUser, string mealGuid, JMeal meal)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JMeal>(validation.Item1, null);
            }

            lock (_lock)
            {
                var existingMeal = _meals.FirstOrDefault(m => m.Guid == mealGuid);
                if (existingMeal == null)
                {
                    var ex = new KeyNotFoundException($"Meal with GUID {mealGuid} not found.");
                    return new Tuple<Exception, JMeal>(ex, null);
                }

                existingMeal.Name = meal.Name;
                existingMeal.Price = meal.Price;
                existingMeal.Discount = meal.Discount;
                existingMeal.Description = meal.Description;
                existingMeal.ImageHash = meal.ImageHash;
                existingMeal.Version += 1.0f;

                // Update in menu if name changed
                if (MainMenu.Meals.Contains(existingMeal.Name))
                {
                    MainMenu.Meals.Remove(existingMeal.Name);
                }
                MainMenu.Meals[existingMeal.Name] = existingMeal;
            }

            LogAction(loggedUser, "Update", "Meal", mealGuid, $"Updated meal: {meal.Name}");
            return new Tuple<Exception, JMeal>(null, meal);
        }

        public Tuple<Exception, bool> RemoveMeal(JUser loggedUser, string mealGuid)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, bool>(validation.Item1, false);
            }

            lock (_lock)
            {
                var meal = _meals.FirstOrDefault(m => m.Guid == mealGuid);
                if (meal == null)
                {
                    var ex = new KeyNotFoundException($"Meal with GUID {mealGuid} not found.");
                    return new Tuple<Exception, bool>(ex, false);
                }

                _meals.Remove(meal);
                if (MainMenu.Meals.Contains(meal.Name))
                {
                    MainMenu.Meals.Remove(meal.Name);
                }
            }

            LogAction(loggedUser, "Delete", "Meal", mealGuid, $"Removed meal: {mealGuid}");
            return new Tuple<Exception, bool>(null, true);
        }

        #endregion

        #region User Operations

        public Tuple<Exception, List<JUser>> GetUsers(JUser loggedUser)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, List<JUser>>(validation.Item1, new List<JUser>());
            }

            LogAction(loggedUser, "Read", "User", "*", "Retrieved all users");
            return new Tuple<Exception, List<JUser>>(null, _users.ToList());
        }

        public Tuple<Exception, JUser> AddUser(JUser loggedUser, JUser user)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JUser>(validation.Item1, null);
            }

            if (user == null)
            {
                var ex = new ArgumentNullException(nameof(user), "User cannot be null.");
                return new Tuple<Exception, JUser>(ex, null);
            }

            user.Guid = GenerateGuid();

            lock (_lock)
            {
                _users.Add(user);
            }

            LogAction(loggedUser, "Create", "User", user.Guid, $"Added user: {user.Name}");
            return new Tuple<Exception, JUser>(null, user);
        }

        public Tuple<Exception, JUser> UpdateUser(JUser loggedUser, string userGuid, JUser user)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JUser>(validation.Item1, null);
            }

            lock (_lock)
            {
                var existingUser = _users.FirstOrDefault(u => u.Guid == userGuid);
                if (existingUser == null)
                {
                    var ex = new KeyNotFoundException($"User with GUID {userGuid} not found.");
                    return new Tuple<Exception, JUser>(ex, null);
                }

                existingUser.Name = user.Name;
                existingUser.Password = user.Password;
                existingUser.UserType = user.UserType;
            }

            LogAction(loggedUser, "Update", "User", userGuid, $"Updated user: {user.Name}");
            return new Tuple<Exception, JUser>(null, user);
        }

        public Tuple<Exception, bool> RemoveUser(JUser loggedUser, string userGuid)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, bool>(validation.Item1, false);
            }

            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Guid == userGuid);
                if (user == null)
                {
                    var ex = new KeyNotFoundException($"User with GUID {userGuid} not found.");
                    return new Tuple<Exception, bool>(ex, false);
                }

                _users.Remove(user);
            }

            LogAction(loggedUser, "Delete", "User", userGuid, $"Removed user: {userGuid}");
            return new Tuple<Exception, bool>(null, true);
        }

        #endregion

        #region Customer Operations

        public Tuple<Exception, List<JCustomer>> GetCustomers(JUser loggedUser)
        {
            var validation = ValidateUser(loggedUser);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, List<JCustomer>>(validation.Item1, new List<JCustomer>());
            }

            LogAction(loggedUser, "Read", "Customer", "*", "Retrieved all customers");
            return new Tuple<Exception, List<JCustomer>>(null, _customers.ToList());
        }

        public Tuple<Exception, JCustomer> AddCustomer(JUser loggedUser, JCustomer customer)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JCustomer>(validation.Item1, null);
            }

            if (customer == null)
            {
                var ex = new ArgumentNullException(nameof(customer), "Customer cannot be null.");
                return new Tuple<Exception, JCustomer>(ex, null);
            }

            customer.Guid = GenerateGuid();

            lock (_lock)
            {
                _customers.Add(customer);
            }

            LogAction(loggedUser, "Create", "Customer", customer.Guid, $"Added customer: {customer.Name}");
            return new Tuple<Exception, JCustomer>(null, customer);
        }

        public Tuple<Exception, JCustomer> UpdateCustomer(JUser loggedUser, string customerGuid, JCustomer customer)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JCustomer>(validation.Item1, null);
            }

            lock (_lock)
            {
                var existingCustomer = _customers.FirstOrDefault(c => c.Guid == customerGuid);
                if (existingCustomer == null)
                {
                    var ex = new KeyNotFoundException($"Customer with GUID {customerGuid} not found.");
                    return new Tuple<Exception, JCustomer>(ex, null);
                }

                existingCustomer.Name = customer.Name;
            }

            LogAction(loggedUser, "Update", "Customer", customerGuid, $"Updated customer: {customer.Name}");
            return new Tuple<Exception, JCustomer>(null, customer);
        }

        public Tuple<Exception, bool> RemoveCustomer(JUser loggedUser, string customerGuid)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, bool>(validation.Item1, false);
            }

            lock (_lock)
            {
                var customer = _customers.FirstOrDefault(c => c.Guid == customerGuid);
                if (customer == null)
                {
                    var ex = new KeyNotFoundException($"Customer with GUID {customerGuid} not found.");
                    return new Tuple<Exception, bool>(ex, false);
                }

                _customers.Remove(customer);
            }

            LogAction(loggedUser, "Delete", "Customer", customerGuid, $"Removed customer: {customerGuid}");
            return new Tuple<Exception, bool>(null, true);
        }

        #endregion

        #region Order Operations

        public Tuple<Exception, List<JOrder>> GetOrders(JUser loggedUser)
        {
            var validation = ValidateUser(loggedUser);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, List<JOrder>>(validation.Item1, new List<JOrder>());
            }

            LogAction(loggedUser, "Read", "Order", "*", "Retrieved all orders");
            return new Tuple<Exception, List<JOrder>>(null, _orders.ToList());
        }

        public Tuple<Exception, JOrder> AddOrder(JUser loggedUser, JOrder order)
        {
            var validation = ValidateUser(loggedUser);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JOrder>(validation.Item1, null);
            }

            if (order == null)
            {
                var ex = new ArgumentNullException(nameof(order), "Order cannot be null.");
                return new Tuple<Exception, JOrder>(ex, null);
            }

            order.Guid = GenerateGuid();
            order.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            lock (_lock)
            {
                _orders.Add(order);
            }

            LogAction(loggedUser, "Create", "Order", order.Guid, $"Added order for customer: {order.CustomerGuid}");
            return new Tuple<Exception, JOrder>(null, order);
        }

        public Tuple<Exception, JOrder> UpdateOrder(JUser loggedUser, string orderGuid, JOrder order)
        {
            var validation = ValidateUser(loggedUser);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, JOrder>(validation.Item1, null);
            }

            lock (_lock)
            {
                var existingOrder = _orders.FirstOrDefault(o => o.Guid == orderGuid);
                if (existingOrder == null)
                {
                    var ex = new KeyNotFoundException($"Order with GUID {orderGuid} not found.");
                    return new Tuple<Exception, JOrder>(ex, null);
                }

                existingOrder.Name = order.Name;
                existingOrder.CustomerGuid = order.CustomerGuid;
                existingOrder.DeliveryStatus = order.DeliveryStatus;
                existingOrder.OrderStatus = order.OrderStatus;
                
                // Update items
                existingOrder.Items.Clear();
                foreach (var key in order.Keys)
                {
                    existingOrder.Items[key] = order[key];
                }
            }

            LogAction(loggedUser, "Update", "Order", orderGuid, $"Updated order: {orderGuid}");
            return new Tuple<Exception, JOrder>(null, order);
        }

        public Tuple<Exception, bool> RemoveOrder(JUser loggedUser, string orderGuid)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, bool>(validation.Item1, false);
            }

            lock (_lock)
            {
                var order = _orders.FirstOrDefault(o => o.Guid == orderGuid);
                if (order == null)
                {
                    var ex = new KeyNotFoundException($"Order with GUID {orderGuid} not found.");
                    return new Tuple<Exception, bool>(ex, false);
                }

                _orders.Remove(order);
            }

            LogAction(loggedUser, "Delete", "Order", orderGuid, $"Removed order: {orderGuid}");
            return new Tuple<Exception, bool>(null, true);
        }

        #endregion

        #region Authentication

        public Tuple<Exception, JUser> Authenticate(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                var ex = new ArgumentException("Name and password are required.");
                return new Tuple<Exception, JUser>(ex, null);
            }

            var user = _users.FirstOrDefault(u => u.Name == name && u.Password == password);
            
            if (user == null)
            {
                var ex = new UnauthorizedAccessException("Invalid credentials.");
                return new Tuple<Exception, JUser>(ex, null);
            }

            LogAction(null, "Authenticate", "User", user.Guid, $"User {name} authenticated");
            return new Tuple<Exception, JUser>(null, user);
        }

        #endregion

        #region Audit Logs

        public Tuple<Exception, List<AuditLog>> GetAuditLogs(JUser loggedUser)
        {
            var validation = ValidateUser(loggedUser, requireAdmin: true);
            if (validation.Item1 != null)
            {
                return new Tuple<Exception, List<AuditLog>>(validation.Item1, new List<AuditLog>());
            }

            return new Tuple<Exception, List<AuditLog>>(null, _auditLogs.ToList());
        }

        #endregion
    }
}