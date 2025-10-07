public abstract class DocumentApp
{
    public abstract TextDocument CreateTextDoc();
    public abstract Spreadsheet CreateSpreadsheet();
}

public class WordApp : DocumentApp
{
    public override TextDocument CreateTextDoc() => new WordDoc();
    public override Spreadsheet CreateSpreadsheet() => new ExcelSheet();
}

public class LibreApp : DocumentApp
{
    public override TextDocument CreateTextDoc() => new WriterDoc();
    public override Spreadsheet CreateSpreadsheet() => new CalcSheet();
}

public interface TextDocument { }
public class WordDoc : TextDocument { }
public class WriterDoc : TextDocument { }

public interface Spreadsheet { }
public class ExcelSheet : Spreadsheet { }
public class CalcSheet : Spreadsheet { }
