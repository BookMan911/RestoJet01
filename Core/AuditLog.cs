using System;

namespace RestoJett.Core
{
    public class AuditLog
    {
        public string Guid { get; set; }
        public string UserGuid { get; set; }
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string EntityType { get; set; }
        public string EntityGuid { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
