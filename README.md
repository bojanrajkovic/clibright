# clibright

`clibright` is a small tool for managing monitor brightness on Windows.

## How

`clibright` relies on existing WinAPI to do the work of speaking DDC-CI to the
monitors and does not actually implement the DDC-CI protocol. These APIs are not
exposed in the Windows UI anywhere, but are available and work with some subset
of monitors.

## Installation

`clibright` is packaged as a .NET Core global tool. Install it by running
`dotnet tool install -g clibright`.

## Uses/usage

`clibright` is intended to be used as a scripting tool with other tools like
AutoHotKey. However, you can also use it directly from the command line:

```shell
clibright set 50 # sets all monitors to 50% brightness
clibright set -i=2 25 # sets the 2nd monitor to 50% brightness via the index argument
clibright get -i=2 # gets brightness for the 2nd monitor
clibright increment 10 # increments brightness on all monitors by 10%
clibright decrement 10 # decrements brightness on all monitors by 10%
clibright list # lists attached monitors and physical monitors--the counter exposed here is the index argument to get/set/increment/decrement
```

## Known working monitors

I don't have a lot of monitors lying around, so I can really only test this on
some subset that I do own. If you've tested this on monitors you own and can
confirm that it works, please send a pull request to update the table!

| Manufacturer | Model        | Firmware Version (optional)  |
|--------------|--------------|------------------------------|
| Dell         | U2718Q       | M2B101                       |
| LG           | 27UD88       |                              |
