namespace SecurityScanFunction.Tests.Interfaces
{
    public interface ICosmosDBSecurityScanner
    {
        Task<ScanResult> ScanAsync(string accountId);
    }

    public class ScanResult
    {
        public string AccountId { get; set; }
        public List<SecurityIssue> Issues { get; set; } = new List<SecurityIssue>();
        public DateTime ScanTime { get; set; }
    }
}