namespace tg_bot_anon_chat
{
    class UserInfo(long Id)
    {
        public long Id { get; } = Id;
        public string? Status { get; set; }
        public long? ConnectedUserId { get; set; }
        public bool IsConnected => ConnectedUserId.HasValue;
    }
}
