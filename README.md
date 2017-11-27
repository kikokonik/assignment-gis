# General course assignment

Build a map-based application, which lets the user see geo-based data on a map and filter/search through it in a meaningfull way. Specify the details and build it in your language of choice. The application should have 3 components:

1. Custom-styled background map, ideally built with [mapbox](http://mapbox.com). Hard-core mode: you can also serve the map tiles yourself using [mapnik](http://mapnik.org/) or similar tool.
2. Local server with [PostGIS](http://postgis.net/) and an API layer that exposes data in a [geojson format](http://geojson.org/).
3. The user-facing application (web, android, ios, your choice..) which calls the API and lets the user see and navigate in the map and shows the geodata. You can (and should) use existing components, such as the Mapbox SDK, or [Leaflet](http://leafletjs.com/).

## Example projects

- Showing nearby landmarks as colored circles, each type of landmark has different circle color and the more interesting the landmark is, the bigger the circle. Landmarks are sorted in a sidebar by distance to the user. It is possible to filter only certain landmark types (e.g., castles).

- Showing bicykle roads on a map. The roads are color-coded based on the road difficulty. The user can see various lists which help her choose an appropriate road, e.g. roads that cross a river, roads that are nearby lakes, roads that pass through multiple countries, etc.

## Data sources

- [Open Street Maps](https://www.openstreetmap.org/)

## My project

Fill in (either in English, or in Slovak):

**Application description**: 

Web application in C# with ASP.NET using REST API with POST method which is used for communication in GeoJSON messages. GeoJSON data are retrieved from Postgis database. Frontend Mapbox SDK map is  showing facilities, respectively private and public schools and medical centers. In addition it is showing intersection areas of radius areas and urban areas of cities. 

Simply said, application is providing this 3 scenarios:
- **Find selected facilities in radius from marker**
- **Find areas of intersection in radius from marker which shows areas in selected distance from schools and medical centers**
- **Find areas of intersection which shows areas in selected distance from schools and medical centers which are in urban areas of city where the marker is**

**Data source**: 

- [Arcgis hub](https://hub.arcgis.com)
    - [Private schools](https://hub.arcgis.com/datasets/DHS-GII::private-schools) 
    - [Public schools](https://hub.arcgis.com/datasets/DHS-GII::public-schools)
    - [Medical centers](https://hub.arcgis.com/datasets/7427f63124164c5aaaad4e5aa62bd3ee_0)
- [Standford Earthworks](https://earthworks.stanford.edu)
    - [Urban areas of cities](https://earthworks.stanford.edu/catalog/stanford-vt734jy6725)

**Technologies used**: 

- C#
- [ASP.NET](https://www.asp.net)
- [PostGIS](http://postgis.net/)
- [Mapbox](http://mapbox.com) and MapBox SDK with styles
