namespace ToDo
{
    public interface IToDoService
    {
        void Save(ToDoItem doItem);
        ICollection<ToDoItem> GetAll();
        ToDoItem? Get(Guid id);
        void Complete(Guid id);
        void Remove(Guid id);
    }
}