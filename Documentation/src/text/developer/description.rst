Description
===========

The #{title} is the tool to set precise geo-coordinates. 

Templates Creating
-------------------
.. table::
    :class: last-specs-table

    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    |              Parameter             |               Description                                                                                                                                                  |
    +====================================+============================================================================================================================================================================+
    | name                               | **Required**. Displayable name of the Template                                                                                                                             |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | code                               | **Required**. Code to detect type of template.  Special codes:                                                                                                             |
    |                                    |  * ``magdrone``. Calculating time if it is not set       in line of data;                                                                                                  |
    |                                    |  * ``nmea``. Uses to parse nmea messages (``GPRMC`` and ``GNRMC`` types are supported).                                                                                    |
    |                                    | Otherwise default parser is used                                                                                                                                           |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | file-type                          | **Required**. The next types of files are                                                                                                                                  |
    |                                    | supported:                                                                                                                                                                 |
    |                                    |  * ``CSV``;                                                                                                                                                                |
    |                                    |  * ``ColumnsFixedWidth``.                                                                                                                                                  |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | match-regex                        | **Required**. Regular expression applies to the   first 10 not empty lines of the file to                                                                                  |
    |                                    | detect template. Example: ^\s*Voltage_End$                                                                                                                                 |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | skip-lines-to                      | Skip lines until the first matching is found. Parameters:                                                                                                                  |
    |                                    |  * ``match-regex``. Regular expression to find a first match;                                                                                                              |
    |                                    |  * ``skip-matched-line``. Skip matched line or  not. Values are ``true`` or ``false``.                                                                                     |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | file-format                        | **Required**. Additional information about file structure. Parameters:                                                                                                     |
    |                                    |  * ``has-header``. Applies to CSV. ``true`` if the file has headers otherwise ``false``;                                                                                   |
    |                                    |  * **Required**. ``comment-prefix``. The symbol used to make a comment;                                                                                                    |
    |                                    |  * **Required**. ``decimal-separator``. The symbol used to separate the integer part from the fractional part of a number written in decimal form. Options are ',' and '.';|
    |                                    |  * ``separator``. Applies to CSV type. The symbol used to separate values from line in CSV                                                                                 |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | data-mapping                       | **Required**. The rules to parse data from file. Parameters:                                                                                                               |
    |                                    |  * **Required**. ``latitude``. The rule to parse latitude data from file. Parameters:                                                                                      |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required**. if header is not set. The serial number of data column;                                                                                      |
    |                                    |    * ``header``. **Required** if index is not set. Calculating serial number of data column using header. has-header must be ``true``;                                     |
    |                                    |    * ``regex``. Parse from data column using regular expression;                                                                                                           |
    |                                    |                                                                                                                                                                            |
    |                                    |  * **Required**. ``longitude``. The rule to parse Longitude data from file. Parameters:                                                                                    |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required** if header is not set. The serial number of data column;                                                                                       |
    |                                    |    * ``header``. **Required** if index is not set. Calculating serial number of data column using ``header``. ``has-header`` must be ``true``;                             |
    |                                    |    * ``regex``. Parse from data column using regular expression;                                                                                                           |
    |                                    |                                                                                                                                                                            |
    |                                    |  * ``date``. The rule to parse Date from file. **Required** if ``date-time`` is not set. Parameters:                                                                       |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required** if header is not set. The serial number of data column;                                                                                       |
    |                                    |    * ``header``. **Required** if index is not set. Calculating serial number of data column using header. ``has-header`` must be ``true``;                                 |
    |                                    |    * ``regex``. Parse from data column using regular expression;                                                                                                           |
    |                                    |    * ``source``. **Required**. Source of date. Options are ``Column`` and  ``FileName``;                                                                                   |
    |                                    |    * ``format``. **Required**. Format of Date.  Example: yyyy-MM-dd.                                                                                                       |
    |                                    |                                                                                                                                                                            |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | data-mapping                       |  * ``time``. The rule to parse Date from file. **Required** if ``date-time`` is not set. Parameters:                                                                       |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required** if header is not set. The serial number of data column;                                                                                       |
    |                                    |    * ``header``. **Required** if index is not set. Calculating serial number of data column using header. ``has-header`` must be ``true``;                                 |
    |                                    |    * ``regex``. Parse from data column using regular expression;                                                                                                           |
    |                                    |    * ``format``. **Required**. Format of Time. Example: HH:mm:ss.fff.                                                                                                      |
    |                                    |                                                                                                                                                                            |
    |                                    |  * ``date-time``. The rule to parse Date from file. **Required** if ``date`` and ``time`` are not set. Parameters:                                                         |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required** if header is not set. The serial number of data column;                                                                                       |
    |                                    |    * ``header``. **Required** if index is not set.  Calculating serial number of data column using header. ``has-header`` must be ``true``;                                |
    |                                    |    * ``regex``. Parse from data column using  regular expression;                                                                                                          |
    |                                    |    * ``format``. **Required**. Format of DateTime. Example: yyyy/MM/dd HH:mm:ss.fff.                                                                                       |
    |                                    |                                                                                                                                                                            |
    |                                    |  * ``trace-number``. The rule to parse trace number from file. Parameters:                                                                                                 |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required**. if header is not set. The serial number of data column;                                                                                      |
    |                                    |    * ``header``. **Required** if index is not set. Calculating serial number of data column using header. has-header must be ``true``;                                     |
    |                                    |    * ``regex``. Parse from data column using regular expression;                                                                                                           |
    |                                    |                                                                                                                                                                            |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
    | data-mapping                       |  * ``timestamp``. The rule to parse timestamp data from file. Applies only to ``magdrone``. Parameters:                                                                    |
    |                                    |                                                                                                                                                                            |
    |                                    |    * ``index``. **Required**. if header is not set. The serial number of data column;                                                                                      |
    |                                    |    * ``header``. **Required** if index is not set. Calculating serial number of data column using header. has-header must be ``true``;                                     |
    |                                    |    * ``regex``. Parse from data column using regular expression;                                                                                                           |
    |                                    |                                                                                                                                                                            |
    +------------------------------------+----------------------------------------------------------------------------------------------------------------------------------------------------------------------------+

       
Example of Template
--------------------
There is an example of mapping SkyHub position.csv::

  name: "SkyHub"
  code: "skyhub" 
  file-type: CSV 
  match-regex: >-
    ^\s*Elapsed,Time,Pitch,Roll,Yaw,Latitude,Longitude,Altitude,RTK Status,ALT:Altitude,GPR:Trace$
  file-format:
    has-header: true
    comment-prefix: '#'
    decimal-separator: '.'
    separator: ','
  data-mapping:
    latitude:
      header: Latitude
    longitude:
      header: Longitude
    time:
      header: Time
      format: 'HH:mm:ss.fff'  
    date:
      source: FileName
      regex: '\d{4}-\d{2}-\d{2}'
      format: 'yyyy-MM-dd'
    trace-number:
      header: "GPR:Trace"
