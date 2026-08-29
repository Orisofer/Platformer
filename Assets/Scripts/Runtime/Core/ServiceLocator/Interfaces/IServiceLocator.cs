
namespace OriGame.Core
{
    public interface IServiceLocator
    {
        public bool Register<T>(T service) where T : class;
    
        public bool Unregister<T>(T service) where T : class;
    
        public T GetService<T>() where T : class;
    }
}

