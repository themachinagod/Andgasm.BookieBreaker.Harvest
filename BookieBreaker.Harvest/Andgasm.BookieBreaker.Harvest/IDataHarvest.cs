
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Harvest
{
    public interface IDataHarvest
    {
        bool CanExecute();
        Task Execute();
    }
}
