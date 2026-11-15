using EnterprisePOS.Domain;

namespace EnterprisePOS.Services
{
    public interface IReceiptPrinter
    {
        void Print(ReceiptData receipt);
        void Preview(ReceiptData receipt);
    }
}
