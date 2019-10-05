
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest
{
    public interface IDataHarvest
    {
        bool CanExecute();
        Task Execute();
    }
}
