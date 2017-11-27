using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;
using System.Text.RegularExpressions;

public partial class Map : Page
{
    protected void Page_Load(object sender, EventArgs e){}


    public static string AddFeatures(string property_name, string amenity, string global_amenity, string raw_description)
    {
        string title = "";
        string description = @",""description"":""" + raw_description  + "\"";

        string color = "";
        string marker_size = "";
        string marker_symbol = "";

        if (global_amenity == "area")
        {
            description = @"{""popupContent"":""" + raw_description + "\"";
            if (amenity == "City Area")
            {
                color = @",""color"": ""#0000ff""";
            }
            else
            {
                color = @",""color"": ""#ff0000""";
            }
        }
        else
        {
            title = @"{""title"":""" + (String.IsNullOrEmpty(property_name) ? "Unknown name" : property_name) + "\"";
            marker_size = @",""marker-size"": ""small""";

            if (global_amenity == "medical_center")
            {
                marker_symbol = @",""marker-symbol"": ""hospital""";
                color = @",""marker-color"": ""#fc4353""";
            }
            else if (global_amenity == "school")
            {
                marker_symbol = @",""marker-symbol"": ""college""";

                if (amenity.ToLower().Contains("elementary"))
                {
                    color = @",""marker-color"": ""#00ff00""";
                }
                else if (amenity.ToLower().Contains("middle"))
                {
                    color = @",""marker-color"": ""#009999""";
                }
                else if (amenity.ToLower().Contains("secondary"))
                {
                    color = @",""marker-color"": ""#009999""";
                }
                else if (amenity.ToLower().Contains("high"))
                {
                    color = @",""marker-color"": ""#0066ff""";
                }
                else if (amenity.ToLower().Contains("high"))
                {
                    color = @",""marker-color"": ""#6600ff""";
                }
                else
                    color = @",""marker-color"": ""#ffff00""";
            }
        }

        string property = String.Format("\"properties\":{0}{1}{2}{3}{4}", title, description, color, marker_size, marker_symbol) + "}";
        return property;
    }
    public static string AddGeometry(string name, string geometry_value, string amenity, string global_amenity, string raw_description)
    {
        if (geometry_value != "")
        {
            string geometry = "\"geometry\":" + geometry_value;
            string json = "{" + String.Format("\"type\": \"Feature\",{0},{1}", geometry, AddFeatures(name, amenity, global_amenity, raw_description)) + "}";
            return json + ",";
        }
        return "";
    }

    private static string FixNumber(string number)
    {
        return Regex.Replace(number, "[^0-9]", "");
    }

    [System.Web.Services.WebMethod]
    public static string ShowCheckedPoints(string currentLat, string currentLong, string distance, string public_count, string private_count, string medical_count, string type)
    {
        try
        {
            distance = FixNumber(distance);
            public_count = FixNumber(public_count);
            private_count = FixNumber(private_count);
            medical_count = FixNumber(medical_count);

            string geojson = "[";

            double numberDistance = Convert.ToDouble(distance);
            numberDistance *= 1000;

            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;Password=1234;Database=PDT;User Id=postgres;Port=5432;");


            try { conn.Open(); }
            catch (Exception e) { }


            NpgsqlCommand cmd;

            string query = "";

            bool publicSchools = type.Contains("public schools");
            bool privateSchools = type.Contains("private schools");
            bool medicalCenters = type.Contains("medical centers");

            if (publicSchools || privateSchools)
            {
                if (publicSchools && privateSchools)
                {
                    query = "(SELECT name, st_asgeojson(wkb_geometry) as geom, level_ as type, naics_desc as desc, telephone as phone " +
                            "FROM public_schools " +
                            "WHERE ST_Distance(ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography, wkb_geometry::geography) <= " + numberDistance.ToString() +
                            " ORDER BY county,city " +
                            "LIMIT " + public_count + " ) " +
                            "UNION ALL " +
                            "(SELECT name, st_asgeojson(wkb_geometry) as geom, level_ as type, naics_desc as desc, telephone as phone " +
                            "FROM private_schools " +
                            "WHERE ST_Distance(ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography, wkb_geometry::geography) <= " + numberDistance.ToString() +
                            " ORDER BY county,city " +
                            "LIMIT " + private_count + " ) ";
                }
                else if (publicSchools)
                {
                    query = "SELECT name, st_asgeojson(wkb_geometry) as geom, level_ as type, naics_desc as desc, telephone as phone " +
                            "FROM public_schools " +
                            "WHERE ST_Distance(ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography, wkb_geometry::geography) <= " + numberDistance.ToString() +
                            " ORDER BY county,city " +
                            "LIMIT " + public_count;
                }
                else
                {
                    query = "SELECT name, st_asgeojson(wkb_geometry) as geom, level_ as type, naics_desc as desc, telephone as phone " +
                            "FROM private_schools " +
                            "WHERE ST_Distance(ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography, wkb_geometry::geography) <= " + numberDistance.ToString() +
                            " ORDER BY county,city " +
                            "LIMIT " + private_count;
                            
                }
                cmd = new NpgsqlCommand("WITH schools as (" + query + " ) " +
                    "SELECT name, geom, type, \"desc\", phone " +
                    "FROM schools" +
                    " LIMIT 300", conn);

                NpgsqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    geojson += AddGeometry(dr["name"].ToString(), dr["geom"].ToString(), dr["type"].ToString(), "school", dr["desc"].ToString() + @"\nPhone:" + dr["phone".ToString()]);//name, geometry_value,amenity, global_amenity,raw_description
                }
                dr.Close();
            }
            if (medicalCenters)
            {
                cmd = new NpgsqlCommand("SELECT hospital_n as name, st_asgeojson(wkb_geometry) as geom, hospital_t as type, hospital_o as desc, phonenum as phone " +
                    "FROM medical_centers " +
                    "WHERE ST_Distance(ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography, wkb_geometry::geography) <= " + numberDistance.ToString() +
                    "ORDER BY county_nam, city_1" +
                    " LIMIT " + medical_count, conn);
                NpgsqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    geojson += AddGeometry(dr["name"].ToString(), dr["geom"].ToString(), dr["type"].ToString(), "medical_center", dr["desc"].ToString());//name, geometry_value,amenity, global_amenity,raw_description
                }
                dr.Close();
            }

            if (geojson.Length == 1)
            {
                geojson = geojson + "]";
            }
            else
            {
                geojson = geojson.Remove(geojson.Length - 1, 1) + "]";
            }

            try { conn.Close(); }
            catch (Exception e) { }

            return geojson;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return "[]";
    }

    [System.Web.Services.WebMethod]
    public static string ShowAreas(string currentLat, string currentLong, string distance, string distanceFromSchool, string distanceFromMedicalCenter, string public_count, string private_count, string medical_count, string type)
    {
        try
        {
            distance = FixNumber(distance);
            public_count = FixNumber(public_count);
            private_count = FixNumber(private_count);
            medical_count = FixNumber(medical_count);
            distanceFromSchool = FixNumber(distanceFromSchool);
            distanceFromMedicalCenter = FixNumber(distanceFromMedicalCenter);

            string geojson = "[";

            double numberDistance = Convert.ToDouble(distance);
            numberDistance *= 1000;

            double numberDistanceFromSchool = Convert.ToDouble(distanceFromSchool) * 1000;
            double numberDistanceFromMedicalCenter = Convert.ToDouble(distanceFromMedicalCenter) * 1000;

            double numberDistanceBetween = numberDistanceFromSchool + numberDistanceFromMedicalCenter;

            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;Password=1234;Database=PDT;User Id=postgres;Port=5432;");


            try { conn.Open(); }
            catch (Exception e) { }


            NpgsqlCommand cmd;

            string query = "";

            query = "SELECT * FROM ((SELECT wkb_geometry::geography as geography " +
                            "FROM public_schools " +
                            "WHERE ST_Distance((select * from point), wkb_geometry::geography) <= " + numberDistance +
                            " ORDER BY county,city " +
                            "LIMIT " + public_count + " ) " +
                            "UNION ALL " +
                            "(SELECT wkb_geometry::geography as geography " +
                            "FROM private_schools " +
                            "WHERE ST_Distance((select * from point), wkb_geometry::geography) <= " + numberDistance +
                            " ORDER BY county,city " +
                            "LIMIT " + private_count + " )) as tab " +
                            "JOIN  " +
                            "(SELECT wkb_geometry::geography as m_geography " +
                            "FROM medical_centers " +
                            "WHERE ST_Distance((select * from point), wkb_geometry::geography) <= " + numberDistance +
                            " ORDER BY county_nam, city_1 " +
                            "LIMIT " + medical_count + " ) as foo2 " +
                            "ON ST_Distance(geography, m_geography) <= " + numberDistanceBetween;

            cmd = new NpgsqlCommand("WITH point as (select ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography as value)" +
                                    "SELECT st_asgeojson(ST_Intersection(ST_Buffer((select * from point)," + numberDistance + ")::geometry, geom)::geography) as geom, ST_Area(geom::geography) as area " +
                                    "FROM " +
                                    "(SELECT ST_Union(geom)::geography as geom " +
                                        "FROM " +
                                    "(WITH properties as (" + query + " ) " +
                                    "SELECT ST_Intersection " +
                                    "   (ST_buffer(properties.geography," + numberDistanceFromSchool + ")::geometry, " +
                                    "   ST_buffer(properties.m_geography," + numberDistanceFromMedicalCenter + ")::geometry)::geometry as geom " +
                                    "FROM properties) as foo) as foo3", conn);


            NpgsqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                geojson += AddGeometry("Area",dr["geom"].ToString(),"","area","Size:"+ dr["area"].ToString()+" m2");//name, geometry_value,amenity, global_amenity,raw_description
            }
            dr.Close();

            if (geojson.Length == 1)
            {
                geojson = geojson + "]";
            }
            else
            {
                geojson = geojson.Remove(geojson.Length - 1, 1) + "]";
            }

            try { conn.Close(); }
            catch (Exception e) { }

            return geojson;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return "[]";
    }

    //SELECT UpdateGeometrySRID('urban_areas','geom',4326);   --- zmena SRID geometrie na 4326, pretoze   ... shp2pgsql -I -s 4269 "...\urban areas\boc_uza.shp" urban_areas | psql -U postgres -d PDT .... 4269 je srid s casovym posunom, na predidenie staleho volania ST_Transform(geom,4326) som vykonal celu transformaciu vopred...
    [System.Web.Services.WebMethod] //TODO PREROB ---obmedz to na city/town kde sa nachadza panacik... sprav prienik vysledku a polygonu city/town
    public static string ShowAreasInCity(string currentLat, string currentLong, string distance, string distanceFromSchool, string distanceFromMedicalCenter, string public_count, string private_count, string medical_count, string type)
    {
        try
        {
            distance = FixNumber(distance);
            public_count = FixNumber(public_count);
            private_count = FixNumber(private_count);
            medical_count = FixNumber(medical_count);
            distanceFromSchool = FixNumber(distanceFromSchool);
            distanceFromMedicalCenter = FixNumber(distanceFromMedicalCenter);

            string geojson = "[";

            double numberDistance = Convert.ToDouble(distance);
            numberDistance *= 1000;

            double numberDistanceFromSchool = Convert.ToDouble(distanceFromSchool) * 1000;
            double numberDistanceFromMedicalCenter = Convert.ToDouble(distanceFromMedicalCenter) * 1000;

            double numberDistanceBetween = numberDistanceFromSchool + numberDistanceFromMedicalCenter;

            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;Password=1234;Database=PDT;User Id=postgres;Port=5432;");


            try { conn.Open(); }
            catch (Exception e) { }


            NpgsqlCommand cmd;

            string query = "";

            query = "SELECT * FROM ((SELECT wkb_geometry::geography as geography " +
                            "FROM public_schools " +
                            "WHERE ST_Intersects((select * from found_area), wkb_geometry::geography) " +
                            " ORDER BY county,city " +
                            "LIMIT " + public_count + " ) " +
                            "UNION ALL " +
                            "(SELECT wkb_geometry::geography as geography " +
                            "FROM private_schools " +
                            "WHERE ST_Intersects((select * from found_area), wkb_geometry::geography)" +
                            " ORDER BY county,city " +
                            "LIMIT " + private_count + " )) as tab " +
                            "JOIN  " +
                            "(SELECT wkb_geometry::geography as m_geography " +
                            "FROM medical_centers " +
                            "WHERE ST_Intersects((select * from found_area), wkb_geometry::geography) " +
                            " ORDER BY county_nam, city_1 " +
                            "LIMIT " + medical_count + " ) as foo2 " +
                            "ON ST_Distance(geography, m_geography) <= " + numberDistanceBetween;

            string findAreaQuery = "found_area as (SELECT geom::geography as value " +
                                    "   FROM urban_areas " +
                                    "   WHERE  ST_intersects((select * from point), urban_areas.geom) " +
                                    "   LIMIT 1 )";

            cmd = new NpgsqlCommand("WITH " +
                        "   point as (select ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography as value), " +
                            findAreaQuery +
                "select st_asgeojson(geom) as geom, ST_Area(geom) as area, amenity from " +
                "(SELECT ST_intersection(ST_Union(geom)::geography,(select * from found_area)) as geom, 'City Area' as amenity " +
                       "FROM " +
                        "(WITH " +
                        "   properties as (" + query + " ) " +
                        "SELECT ST_Intersection" +
                        "   (ST_buffer(properties.geography," + numberDistanceFromSchool + ")::geometry," +
                        "   ST_buffer(properties.m_geography," + numberDistanceFromMedicalCenter + ")::geometry)::geometry as geom " +
                        "FROM properties) as foo" +
                        " UNION" +
                        " select value as geom, 'Found Area' as amenity from found_area) as foo3", conn);

            
            //cmd = new NpgsqlCommand("SELECT st_asgeojson(ST_Union(geom)::geography) as geom, ST_Area(ST_Union(geom)::geography) as area " +
            //                       "FROM " +
            //                        "(WITH " +
            //                        "   point as (select ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography as value), " +
            //                            findAreaQuery +
            //                        "   properties as (" + query + " ) " +
            //                        "SELECT ST_Intersection" +
            //                        "   (ST_Intersection(" +
            //                        "           (select * from found_area),ST_buffer(properties.geography," + numberDistanceFromSchool + ")::geometry)," +
            //                        "   ST_buffer(properties.m_geography," + numberDistanceFromMedicalCenter + ")::geometry)::geometry as geom " +
            //                        "FROM properties) as foo", conn);
            //cmd = new NpgsqlCommand("WITH " +
            //                        "   point as (select ST_MakePoint('" + currentLong + "', '" + currentLat + "')::geography as value), " 
            //                        + findAreaQuery+ " SELECT st_asgeojson(value) as geom, ST_AREA(value) as area FROM found_area", conn);

            NpgsqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                var amenity = dr["amenity"].ToString();
                geojson += AddGeometry("Area", dr["geom"].ToString(), amenity, "area", amenity + @"\nSize: " + dr["area"].ToString() + " m2");//name, geometry_value,amenity, global_amenity,raw_description
            }
            dr.Close();

            if (geojson.Length == 1)
            {
                geojson = geojson + "]";
            }
            else
            {
                geojson = geojson.Remove(geojson.Length - 1, 1) + "]";
            }

            try { conn.Close(); }
            catch (Exception e) { }

            return geojson;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return "[]";
    }



    protected void NumberBox_TextChanged(object sender, EventArgs e)
    {
        TextBox distanceBox = (TextBox)sender;
        distanceBox.Text = FixNumber(distanceBox.Text);
    }
}