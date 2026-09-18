# Asset allocation tracker

## TODO
- If small, medium, large is going to be an exhaustive rule then fixed income and cash have to be assigned
-- small, medum, large, government ???
- Why is everything in `Utils`?
- Shouldn't the importers implement some common interface?
- `IsEq` should go in favor of proper string comparisons
- More robust error detection when the wrong importer is used
- The importers should extract the date
- The etrade importer tests have "invalid data" sets where numeric fields are invalid and cause double.Parse to fail but i don't think that the tiaa or fidelity importers have similar test cases.
- Splitting some of the code out of the `.Import` methods and into static helper functions in each importer would make things easier to read.
- `CreateSegments` needs a bit of cleanup
- Three disabled CommandLine tests
- Move some of the hardcoded constants inside `CreateLegend` into the config class
- Need to specify a `viewbox` in the `<svg>` but to do this i need to calculate the far right edge of the text
- This thing produces indivdual pie charts but it really needs to produce them, as well as tabular data, in the form of a report


## Evaluation of possible designs
I decided to go with Design 1.  This section is just here for historical purposes.

### Design 1:  Flat list of categories (WINNER)
- Flat list of categories, each entity can belong to multiple categories
- Plot is constructed by &&'ing together sets of categories that define each slice
 - An entity contributing to the slice must be a member of all the categories &&'ed together
- It's easy to create a chart that misses entities
 - But OTOH this is a feature; you might want to visualize equity w/o fixed income
 - Some sort of assertion system to prevent mistakes?
- Some sort of rule system do define things like incompatible category combinations
 - domestic, international
 - equity, fixed income
 - ...

### Design 2:  Multiple category sets (LOSER)
- Ex
 - Provenance: domestic, international
 - Type: equity, fixed income, cash (?)
 - Size: large, medium, small
- Every entity must be annotated with exactly one entry from each category set
- Not every category set might apply cleanly to every entity
 - Are govt bonds "large"?
 - Is cash large, medium, or small?
- Makes it difficult to do oddball categories like pme, reit





