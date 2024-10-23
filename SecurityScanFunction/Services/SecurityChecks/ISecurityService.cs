using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SecurityScanFunction.Services.Security
{
    public interface ISecurityService
    {
        Task<ScanResult> PerformSecurityCheck(string resourceId, string resourceName);
    }
}