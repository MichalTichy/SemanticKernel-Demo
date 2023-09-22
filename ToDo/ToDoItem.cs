namespace ToDo
{
	public record ToDoItem(Guid Id, string Description, TimeSpan? ExpectedDuration, DateTime? Due, IEnumerable<string> Tags)
	{
		public DateTime? CompletedAt { get; private set; }
		public void MarkAsCompleted() => CompletedAt = DateTime.Now;
	}
}