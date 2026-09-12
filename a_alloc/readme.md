# Asset allocation tracker

## TODO
- If small, medium, large is going to be an exhaustive rule then fixed income and cash have to be assigned
-- small, medum, large, government ???
- Why is everything in `Utils`?



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





