namespace tg_bot_anon_chat
{
    class UserManager
    {
        private static readonly List<UserInfo> users = [];

        public static bool UserExists(long userId) => users.FirstOrDefault(u => u.Id == userId) is not null;

        public static UserInfo? GetUser(long userId) => users.FirstOrDefault(u => u.Id == userId);

        public static List<UserInfo> GetAllUsers => users;

        public static int Count => users.Count;

        public static void AddUser(long userId)
        {
            if (!UserExists(userId))
                users.Add(new UserInfo(userId));
        }

        public static void ConnectUsers(long firstUserId, long secondUserId)
        {
            var user1 = GetUser(firstUserId);
            var user2 = GetUser(secondUserId);

            if (user1 != null && user2 != null)
            {
                user1.ConnectedUserId = secondUserId;
                user2.ConnectedUserId = firstUserId;
            }
        }

        public static void DisconnectUsers(long userId)
        {
            var user = GetUser(userId);

            if (user != null)
            {
                if (user.ConnectedUserId.HasValue)
                {
                    UserInfo? value = GetUser(user.ConnectedUserId.Value);
                    if (value != null)
                    {
                        value.ConnectedUserId = null;
                        user.ConnectedUserId = null;
                    }
                }
            }
        }
    }
}
