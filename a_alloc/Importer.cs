


interface Importer
{
    // IsValid() checks for specific markers that identify a file as a certain type, but does NOT perform
    // exhaustive validation.  IsValid()==true means that the input is "advertising" itself as being compatible
    // with a particular importer.  Import() may still fail if there are errors, corruptions, or other problems
    // elsewhere in the file.  IsValid()==false means you have created the wrong importer for the given file.
    // IsValid()==true && Import()==null means that there is a problem with the file, and that a different
    // importer is not the solution.
    bool IsValid(string[] lines);

    // The data that the data were obtained
    DateTime? Date(string[] lines);

    
    List<Utils.Asset>? Import(string[] lines);
}












