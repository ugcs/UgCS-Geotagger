User Interface
==============

Preparation
-----------

The #{title} contains two tables (See the image below):
  * ``#{psf}`` to add files which contain precise data;
  * ``#{ftu}`` to add files which data has to be replaced.

.. figure:: ugcs-ppk.PNG
   :width: 120mm

   The #{title} Main Window

To work with files Interface has the following buttons:
  * ``Add`` to add new Position Solution File or File To Update. You can add several files at once;
  * ``Remove`` to remove new Position Solution File or File To Update. You can remove files only one by one;
  * ``Add Folder`` to add the folder containing all necessary files. The #{title} distributes files automatically;
  * ``Clear`` to remove all files from tables;
  * ``Process Files`` to start replacing data.

``#{psf}`` grid has the next columns:
  * ``Name``. The name of added file;
  * ``Type``. Displayable name of the used template to parse data;
  * ``#{start-time}``. The minimal time recorded in the file;
  * ``#{end-time}``. The maximal time recorded in the file.

``#{ftu}`` grid has the next columns:
  * ``Name``. The name of added file;
  * ``Type``. Displayable name of the used template to parse data;
  * ``#{start-time}``. The minimal time recorded in the file;
  * ``#{end-time}``. The maximal time recorded in the file;
  * ``Linked File``. Applies only to SkyHub position.csv with GPR recorded data. The #{title} search by name of the file linked .sgy file and geo-coordinates of the linked also will be replaced after processing;
  * ``Coverage Status``. There can be the next statuses :

      * ``Not Covered``. If there is no coverage by time of ``#{ftu}`` from ``#{psf}`` table. It means that ``#{start-time}`` and ``#{end-time}`` of file do not intersect with ``#{psf}``.
      * ``Partially Covered``. If only part of data of ``#{ftu}`` covered by time from ``#{psf}`` tables. It means that ``#{start-time}`` and ``#{end-time}`` of file do not completely intersect.
      * ``Not Covered``. If there is a full coverage for ``#{ftu}`` from ``#{psf}`` grid. It means that ``#{start-time}`` and ``#{end-time}`` of file completely intersect.

Processing
----------
When all necessary files are added you can start replacing geo-coordinates.

Processing has 2 stages:
  1. Interpolating. The #{title} calculates geo-coordinates for every time from ``#{ftu}``;  
  2. Writing data in new file. The #{title} creates the new file with replaced coordinates. The new file contains '-ppk' suffix in the file name. File is created in the same folder.


