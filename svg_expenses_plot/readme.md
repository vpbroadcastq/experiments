# Monthly spending plot with $10/day target
Plot your cumulative monthly spending relative to a target of $10/day

## Building and running
`dotnet build && ./bin/Debug/net10.0/svg ./may.txt > example2.svg`


## Questions
- `TransactionsToPlotData` returns a `List<Point>`.  Is there a more generic interface?  Return an `IEnumerable<Point>`?  What would best practice be?  Note that i accept a `Span`.  Returning an `IEnumerable<Point>` would erase the array-ness of `List`.  Is there a way to return a `Span` (would need to extend the lifetime of the backing `List`)?


## Notes
- All of the `Create` functions that emit `XElement`s have inputs in abs/pixel space.  None of these methods should accept paremeters in plot space (days, $).


## TODO
- Tick labels
- Axis font is ugly
- User-configurable size, styles
- Output control
- Configurable target spend
- Multiple targets?



