# Noodle host adapters

The C# application uses Windows Forms, timers, drawing, files, resources, and database providers. These are host/platform integrations, not pure simulation logic. Noodle migration must define interfaces for UI events, rendering, clocks/timers, file access, configuration storage, database connections, and application startup, then implement adapters for the chosen runtime.

The Noodle modules in this directory are contracts and migration scaffolds only; they do not yet reproduce the WinForms application or its full interaction model.
