namespace RestoJett.Core
{
    public class JUser
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Guid { get; set; }
        public JUserType UserType { get; set; }
    }
}