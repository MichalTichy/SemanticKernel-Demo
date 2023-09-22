namespace ToDo
{
    public class ToDoService : IToDoService
    {
		private readonly Dictionary<Guid, ToDoItem> todoItems = new();

		public void Save(ToDoItem toDoItem)
		{
            if (toDoItem.Id==Guid.Empty)
            {
                toDoItem = toDoItem with {Id = Guid.NewGuid()};
            }
            todoItems[toDoItem.Id] = toDoItem;
		}

		public ICollection<ToDoItem> GetAll() => todoItems.Values;

		public ToDoItem? Get(Guid id) => todoItems.GetValueOrDefault(id);

		public void Complete(Guid id)
		{
			todoItems.TryGetValue(id, out var item);
			item?.MarkAsCompleted();
		}
		
		public void Remove(Guid id) => todoItems.Remove(id);
	}
}