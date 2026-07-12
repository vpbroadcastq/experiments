## Questions
- `TransactionsToPlotData` returns a `List<Point>`.  Is there a more generic interface?  Return an `IEnumerable<Point>`?  What would best practice be?  Note that i accept a `Span`.  Returning an `IEnumerable<Point>` would erase the array-ness of `List`.  Is there a way to return a `Span` (would need to extend the lifetime of the backing `List`)?




## Notes
- All of the `Create` functions that emit `XElement`s have inputs in abs/pixel space.  None of these methods should accept paremeters in plot space (days, $).






