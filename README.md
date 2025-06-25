# MAVLink Unity binding

This is a Unity binding for the MAVLink library. It is based on Mission Planner (Simple Exmaple) and the MAVLink
library.

## Installation

## Generate Dialect

## Download Message Definitions

## Example

## Design Specs

## How to connect:

#### Serial

- Linux: Serial:///dev/ttyACM0 or Serial:///dev/ttyUSB0
- Windows:

#### Resource management

- C# uses tracing GC instead of reference counting (as in rust `RC<>`) which makes it impossible to implement
  nativeRAII, thus, it is recommended to associate lifetime of resources with a Unity `GameObject`
- One example is a UITK ListView that contains all `Cleanable` under its registry, implemented as a PreFab
- ListView item should have a close button in case of emergency
- ListView item should have a colored emoji to show its health

#### Routing

- Test il2cpp compatibility by QGroundControl + UDP port
- Add arpx FFI
- Add arpx yaml schema to automatically download and launch SITL + mavlink-router (OS dependent) in test

#### UI

- 1 button for zeroing the AR glasses
- 3 buttons to load Resources (MAVLink Routing, AR glasses, VLC Feed) to the ListView
    - each button use a file picker to load resource definition
- 1 buttons to batch-load Resources defined in file that refers to other files
