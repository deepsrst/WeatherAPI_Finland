
//search city case
var url_string = window.location;
var url = new URL(url_string);
var city = url.searchParams.get("city");
if (city == null) {
    city = "helsinki";
}

const now = new Date();
//fectching the data of Tameside city using fetch
fetch('https://api.openweathermap.org/data/2.5/weather?q='+ city +'&appid=4f9643e46edacb413a1e3aed520da247&units=metric')
    .then(function (response) {
        return response.json()
    })
    .then(function (response) {
       // declaring variables for template string 
        var app = '';
        //creating date and time for Tameside city
        let d = new Date()
        let localTime = d.getTime()
        let localOffset = d.getTimezoneOffset() * 60000
        let utc = localTime + localOffset
        let tameside = utc + (1000 * response.timezone)
        let tamedate = new Date(tameside)

        let time = tamedate.toLocaleString('en-US', { month: "long", weekday: "long", day: "numeric", hour: 'numeric', minute: 'numeric', hour12: true })

        //temlate literal
        app += `
        <div class="container">
            <div class="datetime">${time}</div>
            <h1 class="city">${response.name}, ${response.sys.country}</h1>
            <div class="weather-detail">
                <img src="http://openweathermap.org/img/wn/${response.weather[0].icon}@2x.png" alt="${response.weather[0].main}"/> 
                <span class="temp">${Math.round(response.main.temp)}°C</span>
            </div>
            <div class="weather-desc">
                <div class="main-weather">${response.weather[0].main}</div>
                <p>Feels like ${Math.round(response.main.feels_like)}°C. ${response.weather[0].description}</p>
            </div>
            <ul class="weather-info">
                <li><span class="min">Min: ${Math.round(response.main.temp_min)}°C to Max: ${Math.round(response.main.temp_max)} °C</span></li>
                <li class="windspeed">Windspeed : ${response.wind.speed} m/s</li>
                <li class="humidity">Humidity : ${response.main.humidity}%</li>
                <li class="visibility">Visibility : ${Math.round(response.visibility / 1000)} km</li>
            </ul>
        </div>
      `

        document.querySelector('#app').innerHTML = app;

    })
    //catch block for errors
    .catch(function (error) {
        document.querySelector('#app').innerHTML = '<div class="container error"><p>' + error + '</p> <p>Please check your network connection or API config .</p> </div>';

    })