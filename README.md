# CsvTools

Collection of tools for modifying and analyzing CSV files.

## Tools

|Action|Functionality|
| :---: | :---: |
|`delete-line`|Deletes lines that fit the specified condition|
|`delete-column`|Deletes chosen columns|
|`analyze`|Outputs statistics for chosen columns|

## Usage

Use the following syntax to execute one of the actions:
```
CsvTools [action] [--arguments]
```

Use help for information about available arguments:
```
CsvTools help [action]
```

## Functionality

All standard operators can be used when forming expressions with actions that support them. More details available [here](https://ncalc.github.io/ncalc/articles/operators.html).

### Column identifiers

"Column identifiers" or simply "identifiers" refer to names which columns are referred by. By default they are assigned to the value of that column in the first line.

If the first line of your CSV doesn't contain identifier values and instead contains regular data, use `--ignore-identifiers`. In this case identifier names will be assigned like this: `Column1`, `Column2`, `Column3`...

### Data types

It is assumed that all data in a column is of the same type (string or double).

In conditions, enclose strings in single quotes ('). You can escape a single quote inside a string with a backslash (\\') or another single quote ('').

### Functions

Functions are used in the following format: `FunctionName(arg1, arg2...)`

To read a double value as a string, use: `ToString(value)`

To check wether a string contains another string, use: `Contains(string, string)`

List of other supported functions is available [here](https://ncalc.github.io/ncalc/articles/functions.html#built-in-functions).

## Examples

- Delete all lines where the value of `ExampleColumn` is larger than 5
```cmd
CsvTools delete-line --input-file "Example.csv" --condition "ExampleColumn > 5"
```

- Delete all lines where the `Column1` value contains letters A and B:
```cmd
CsvTools delete-line --input-file "Example.csv" --condition "Contains(Column1, 'A') && Contains(Column1, 'B')"
```

- Delete the `Column1` and `Column2` columns
```cmd
CsvTools delete-column --input-file "Example.csv" --identifiers Column1,Column2
```
