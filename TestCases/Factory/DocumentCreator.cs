namespace FactoryMethodExample
{
    // 1️⃣ Product interface
    public interface IDocument
    {
        void Print();
    }

    // 2️⃣ Concrete Products
    public class DefaultDocument : IDocument
    {
        public void Print() => Console.WriteLine("Printing default document...");
    }

    public class PdfDocument : IDocument
    {
        public void Print() => Console.WriteLine("Printing PDF document...");
    }

    // 3️⃣ Creator (concrete but extensible)
    public class DocumentCreator
    {
        // Factory Method — virtual so subclasses can override
        public virtual IDocument CreateDocument() => new DefaultDocument();

        public void Render()
        {
            var doc = CreateDocument();
            doc.Print();
        }
    }

    // 4️⃣ Concrete Creator
    public class PdfCreator : DocumentCreator
    {
        public override IDocument CreateDocument() => new PdfDocument();
    }
}
