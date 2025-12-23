using System.Collections.Generic;

namespace GbtpLib.Security
{
    public class UserContext
    {
        public string SiteCode { get; set; }          // MST_USER_INFO.SITE_CD
        public string UserId { get; set; }            // MST_USER_INFO.USER_ID
        public string UserName { get; set; }          // MST_USER_INFO.USER_NM

        public string UserGroupCode { get; set; }     // MST_USER_INFO.USER_GROUP_CD
        public string DepartmentCode { get; set; }    // MST_USER_INFO.DEPT_CD
        public string Sex { get; set; }               // MST_USER_INFO.SEX
        public int? ListOrder { get; set; }           // MST_USER_INFO.LIST_ORDER
        public string UseYn { get; set; }             // MST_USER_INFO.USE_YN

        public string Role { get; set; }
        public IDictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }

    public interface IUserContextProvider
    {
        UserContext Current { get; }
        void Set(UserContext context);
        void Clear();
    }

    public class ThreadStaticUserContextProvider : IUserContextProvider
    {
        [System.ThreadStatic]
        private static UserContext _current;
        public UserContext Current => _current;
        public void Set(UserContext context) => _current = context;
        public void Clear() => _current = null;
    }
}
