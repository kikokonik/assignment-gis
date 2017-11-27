<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Map.aspx.cs" Inherits="Map" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>PDT projekt Koník</title>
    <meta name='viewport' content='initial-scale=1,maximum-scale=1,user-scalable=no' />
    <script src='https://api.mapbox.com/mapbox.js/v3.1.1/mapbox.js'></script>
    <link href='https://api.mapbox.com/mapbox.js/v3.1.1/mapbox.css' rel='stylesheet' />
    <link rel="stylesheet" type="text/css" href="style.css">
    <link rel="stylesheet" href="//code.jquery.com/ui/1.11.4/themes/smoothness/jquery-ui.css">
    <script src="http://code.jquery.com/jquery-latest.min.js"></script>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap.min.css" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap-theme.min.css" />
    <link rel="stylesheet" href="https://ajax.googleapis.com/ajax/libs/jqueryui/1.11.4/themes/smoothness/jquery-ui.css">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js" type="text/javascript"></script>
</head>
<body >
    <form id="form1" runat="server">
		<div id="menu">
			<p>
				<asp:Label ID="checkBoxLabel" runat="server" Text="Facilities types (works for first scenario button)"></asp:Label>
			</p>
			<nav id='checkBoxesMenu' class='checkBoxesMenu-ui'></nav>
			<p>
				<asp:Label ID="distanceLabel" runat="server" Text="Max distance (km)"></asp:Label>
			</p>
			<p>
				<asp:TextBox ID="distanceBox" runat="server" OnTextChanged="NumberBox_TextChanged" MaxLength="4">100</asp:TextBox>
			</p>
			<p>
				<asp:Label ID="publicCountBoxLabel" runat="server" Text="Items of public schools"></asp:Label>
			</p>
			<p>
				<asp:TextBox ID="publicCountBox" runat="server" OnTextChanged="NumberBox_TextChanged" MaxLength="3">100</asp:TextBox>
			</p>
			<p>
				<asp:Label ID="privateCountLabel" runat="server" Text="Items of private schools"></asp:Label>
			</p>
			<p>
				<asp:TextBox ID="privateCountBox" runat="server" OnTextChanged="NumberBox_TextChanged" MaxLength="3">100</asp:TextBox>
			</p>
			<p>
				<asp:Label ID="medicalCountLabel" runat="server" Text="Items of medical centers"></asp:Label>
			</p>
			<p>
				<asp:TextBox ID="medicalCountBox" runat="server" OnTextChanged="NumberBox_TextChanged" MaxLength="3">100</asp:TextBox>
			</p>
			<p>
				<button type="button" onclick="ShowCheckedPointsUpdate()">Search facilities</button>
			</p>
			<p>
				<asp:Label ID="distanceFromSchoolLabel" runat="server" Text="Max distance from school (km)"></asp:Label>
			</p>
			<p>
				<asp:TextBox ID="distanceFromSchoolBox" runat="server" OnTextChanged="NumberBox_TextChanged" MaxLength="4">10</asp:TextBox>
			</p>
			<p>
				<asp:Label ID="distanceFromMedicalCenterLabel" runat="server" Text="Max distance from medical center (km)"></asp:Label>
			</p>
			<p>
				<asp:TextBox ID="distanceFromMedicalCenterBox" runat="server" OnTextChanged="NumberBox_TextChanged" MaxLength="4">10</asp:TextBox>
			</p>
			<p>
				<button title="Find areas which are selected Max distance far from you (dragable man mark) and selected max distance from School and Medical center" type="button" onclick="ShowAreasUpdate()">Search areas in radius</button>
			</p>
			<p>
				<button title="Find areas which are selected in city you (dragable man mark) are in and selected max distance from School and Medical center" type="button" onclick="ShowAreasInCityUpdate()">Search areas in city</button>
			</p>
		</div>
		
        <div id="map">			
			<asp:Image ID="Image1" runat="server" ImageUrl="~/Models/Llgbv.gif" CssClass="img" />
            <script type="text/javascript">               
				var checkboxes = [];
				var types = ['public schools', 'private schools', 'medical centers'];
				var checkBoxes = types.join(',');

				var null_geojson = jQuery.parseJSON('[]');
				var lastObjectsLayer;

				var lastFunction = "ShowCheckedPointsUpdate";

				var distance = document.getElementById('distanceBox');
				var publicCount = document.getElementById('publicCountBox');
				var privateCount = document.getElementById('privateCountBox');
				var medicalCount = document.getElementById('medicalCountBox');
				var distanceFromSchool = document.getElementById('distanceFromSchoolBox');
				var distanceFromMedicalCenter = document.getElementById('distanceFromMedicalCenterBox');

				L.mapbox.accessToken = 'pk.eyJ1Ijoia2lrb2tvbmlrIiwiYSI6ImNqOWJxdzVkNzFiOXcyd3BhMmZxc3I2ZTkifQ.niPIG-a1wkpQs0dZP4iXBA';

				var map = L.mapbox.map('map', 'mapbox.light').setView([38.636522, -95.334442], 4);

				var currentPosition = L.marker([38.636522, -95.334442], {
					icon: L.mapbox.marker.icon({
						'marker-color': '#3bb2d0',
						'marker-size': 'large',
						'marker-symbol': 'pitch'
					}),
					draggable: true
				}).addTo(map);

				checkBoxesMenuInit();

				currentPosition.on('dragend', refreshPosition);
				refreshPosition();

				function refreshPosition() {
					currentPosition.bindPopup('<b> Current position </b><br>' + currentPosition.getLatLng().lat + '<br>' + currentPosition.getLatLng().lng);
					if (lastFunction == "ShowCheckedPointsUpdate") {
						ShowCheckedPointsUpdate();
					}
					else if (lastFunction == "ShowAreasUpdate") {
						ShowAreasUpdate();
					}
					else if (lastFunction == "ShowAreasInCityUpdate") {
						ShowAreasInCityUpdate();
					}
				}

				function ShowCheckedPoints() {
					$.ajax({
						type: "POST",
						async: true,
						processData: true,
						cache: false,
						url: 'Map.aspx/ShowCheckedPoints',
						data: '{"currentLat":"' + currentPosition.getLatLng().lat + '","currentLong":"' + currentPosition.getLatLng().lng +
						'","distance":"' + distance.value + '","public_count":"' + publicCount.value + '","private_count":"' + privateCount.value + '","medical_count":"' + medicalCount.value + '","type":"' + checkBoxes + '"}',
						contentType: 'application/json; charset=utf-8',
						dataType: "json",
						success: function (data) {
							try {
								HideLoadingDiv();
								var geojson = jQuery.parseJSON(data.d);
								map.featureLayer.setGeoJSON(geojson);
							}
							catch (err) {
								console.log(err.message);
								console.log(data.d);
							}
						},
						error: function (err) {
							HideLoadingDiv();
							console.log(err.message);
						}
					});
				}


				function ShowAreas() {
					$.ajax({
						type: "POST",
						async: true,
						processData: true,
						cache: false,
						url: 'Map.aspx/ShowAreas',
						data: '{"currentLat":"' + currentPosition.getLatLng().lat + '","currentLong":"' + currentPosition.getLatLng().lng +
						'","distance":"' + distance.value + '","distanceFromSchool":"' + distanceFromSchool.value + '","distanceFromMedicalCenter":"' + distanceFromMedicalCenter.value + '","public_count":"' + publicCount.value + '","private_count":"' + privateCount.value + '","medical_count":"' + medicalCount.value + '","type":"' + checkBoxes + '"}',
						contentType: 'application/json; charset=utf-8',
						dataType: "json",
						success: function (data) {
							try {
								HideLoadingDiv();
								var geojson = jQuery.parseJSON(data.d);
								map.featureLayer.setGeoJSON(null_geojson);
								if (lastObjectsLayer != null)
								{
									map.removeLayer(lastObjectsLayer);
								}
								lastObjectsLayer = L.geoJSON(geojson, {
									onEachFeature: onEachFeature
								}).addTo(map);
							}
							catch (err) {
								console.log(err.message);
								console.log(data.d);
							}
						},
						error: function (err) {
							HideLoadingDiv();
							console.log(err.message);
						}
					});
				}

				function ShowAreasInCity() {
					$.ajax({
						type: "POST",
						async: true,
						processData: true,
						cache: false,
						url: 'Map.aspx/ShowAreasInCity',
						data: '{"currentLat":"' + currentPosition.getLatLng().lat + '","currentLong":"' + currentPosition.getLatLng().lng +
						'","distance":"' + distance.value + '","distanceFromSchool":"' + distanceFromSchool.value + '","distanceFromMedicalCenter":"' + distanceFromMedicalCenter.value + '","public_count":"' + publicCount.value + '","private_count":"' + privateCount.value + '","medical_count":"' + medicalCount.value + '","type":"' + checkBoxes + '"}',
						contentType: 'application/json; charset=utf-8',
						dataType: "json",
						success: function (data) {
							try {
								HideLoadingDiv();
								var geojson = jQuery.parseJSON(data.d);
								map.featureLayer.setGeoJSON(null_geojson);
								if (lastObjectsLayer != null) {
									map.removeLayer(lastObjectsLayer);
								}
								lastObjectsLayer = L.geoJSON(geojson, {
									style: function (feature) {
										return { color: feature.properties.color }
									},
									onEachFeature: onEachFeature
								}).addTo(map);
							}
							catch (err) {
								console.log(err.message);
								console.log(data.d);
							}
						},
						error: function (err) {
							HideLoadingDiv();
							console.log(err.message);
						}
					});
				}

				function checkBoxesMenuInit() {
					for (var i = 0; i < types.length; i++) {
						var item = checkBoxesMenu.appendChild(document.createElement('div'));
						var checkbox = item.appendChild(document.createElement('input'));
						var label = item.appendChild(document.createElement('label'));
						checkbox.type = 'checkbox';
						checkbox.id = types[i];
						checkbox.checked = true;
						label.innerHTML = types[i];
						label.setAttribute('for', types[i]);
						checkboxes.push(checkbox);
					}
				}

				function ShowCheckedPointsUpdate() {
					DisplayLoadingDiv();

					checkBoxes = '';
					for (var i = 0; i < checkboxes.length; i++) {
						if (checkboxes[i].checked) {
							checkBoxes += types[i] + ',';
						}
					}
					checkBoxes = checkBoxes.substr(0, checkBoxes.length - 1);
					lastFunction = "ShowCheckedPointsUpdate";
					ShowCheckedPoints();
				}

				function ShowAreasUpdate() {
					DisplayLoadingDiv();

					checkBoxes = '';
					for (var i = 0; i < checkboxes.length; i++) {
						if (checkboxes[i].checked) {
							checkBoxes += types[i] + ',';
						}
					}
					checkBoxes = checkBoxes.substr(0, checkBoxes.length - 1);
					lastFunction = "ShowAreasUpdate";
					ShowAreas();
				}

				function ShowAreasInCityUpdate() {
					DisplayLoadingDiv();

					checkBoxes = '';
					for (var i = 0; i < checkboxes.length; i++) {
						if (checkboxes[i].checked) {
							checkBoxes += types[i] + ',';
						}
					}
					checkBoxes = checkBoxes.substr(0, checkBoxes.length - 1);
					lastFunction = "ShowAreasInCityUpdate";
					ShowAreasInCity();
				}

				function DisplayLoadingDiv() {

					var img1 = document.getElementById('Image1');
					img1.style.visibility = 'visible';

				}
				function HideLoadingDiv() {
					var img1 = document.getElementById('Image1');
					img1.style.visibility = 'hidden';
				}

				function onEachFeature(feature, layer) {
					// does this feature have a property named popupContent?
					if (feature.properties && feature.properties.popupContent) {
						layer.bindPopup(feature.properties.popupContent);
					}
				}


			</script>
        </div>
    </form>
</body>
</html>
