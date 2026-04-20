namespace FCG.Domain.Shared
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public bool IsActive { get; protected set; }

        protected Entity()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
