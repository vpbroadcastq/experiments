



// Kinda like std::recursive_directory_iterator
// If the directory tree that would be iterated over changes after creation, the behavior is undefined.
class FsIterator
{
    List<string> dirs_;
    IEnumerator<string>? curr_dir_;

    private FsIterator(List<string> dirs, IEnumerator<string>? curr)
    {
        dirs_ = dirs;
        curr_dir_ = curr;
    }

    // Creates an FsIterator if the input is a valid path that exists on the system.  Returns null otherwise.
    public static FsIterator? Create(string path)
    {
        if (Directory.Exists(path))
        {
            List<string> dirs = new List<string>();
            dirs.Add(path);
            FsIterator fsIt =  new FsIterator(dirs, null);
            fsIt.Next(); // Current() won't work until curr_dir_ is populated by Next()
            return fsIt;
        }
        // path might exist, but is not be a directory
        if (!Path.Exists(path))
        {
            return null;
        }
        // Exists, but is not a directory.  Return an FsIterator with an empty dirs_ and an iterator
        // pointing into a List<string> holding the single entry.  The IEnumerator keeps the single-entry
        // List alive.
        List<string> dummy = new List<string>();
        dummy.Add(path);
        IEnumerator<string> it = dummy.GetEnumerator();
        it.MoveNext(); // TODO:  Check return?
        return new FsIterator(new List<string>(), it);
    }

    public string? Current()
    {
        if (curr_dir_ == null)
        {
            return null; // Corresponds to completed iteration
        }
        return curr_dir_.Current;
    }

    public bool Next()
    {
        // If current is a directory it needs to be pushed onto the stack before moving past it
        string? current = Current();
        if (current != null && Directory.Exists(current))
        {
            dirs_.Add(current);
        }

        // Now actually move next
        if (curr_dir_ != null && curr_dir_.MoveNext())
        {
            return true; // Enumerator not exhausted
        }
        // Enumerator exhausted OR enumerator was never populated (happens on a new object)
        if (dirs_.Count() == 0)
        {
            return false; // Iteration complete
        }

        curr_dir_ = Directory.EnumerateFileSystemEntries(dirs_.Last()).GetEnumerator();
        curr_dir_.MoveNext(); // TODO:  Check return?
        dirs_.RemoveAt(dirs_.Count()-1);
        return true;
    }

    public bool Finished()
    {
        // Bug:  Nothing ever nulls curr_dir_
        return (curr_dir_ == null) || (curr_dir_.Current == null && dirs_.Count() == 0);
    }
}




