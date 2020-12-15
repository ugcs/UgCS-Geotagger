Supported Types
===============

The #{title} supports the next types by default:

  * #{ftu}: 

    * SkyHub position.csv log with GPR recorded data;
    * SkyHub position.csv log with GNSS recorded data;
    * NMEA log (GPRMC and GNRMC messages) created by SkyHub;
    * MagDrone data recorded via csv file;
    * MagArrow data recorded via csv file.

  * #{ftu}:

    * RTKLib .pos files;
    * SkyHub position.csv log with GPR recorded data;
    * SkyHub position.csv with NMEA recorded data;
    * SkyHub position.csv with Altimeter data.

**Important note**: If SkyHub position.csv contains GPR recorded data and it is File to Update then it can update linked .sgy file, created by SkyHub.

**Important note 2**: You can write your own template if you want to support another types of files. See ``ugcs-ppk-developer-manual``.  

