Kerbal Data Output is a data export program for [Kerbal Space Progra](http://kerbalspaceprogram.com)
It's currently in it's early stages, so please bear with me for any bugs and such.

Data Exported
=============
This is the data it exports, in no particular order.


/vessels/all
------------
This exports information on all the vessels in the simulation.

/vessels/active
---------------
Information on the active vessel.

/vessels/by-id/<id>
-------------------

Information on the given vessel ID. You can get this ID from either of the two above vessels data export paths, in the JSON 
output of the "id" field.

/bodies/all
-----------
Data on all the celestrial bodies in the simulation.

/bodies/by-name/<name>
----------------------
Data on the given celestrial body.

/sim
----

Information on the simulation, such as the game's warp rate and paused/unpaused state.

How to get the data
===================

As of now, there's no pretty graphs or anything, as I've been focusing on the JSON api itself. To get data out you just need to
go to http://localhost:8080<path> -- where <path> is the given data export above.