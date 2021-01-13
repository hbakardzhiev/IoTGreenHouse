Vue.component('axios-requests', {
  template: `
<div>
    <div>
        <button @click="getPhoto">Get best performing</button>
        <button @click="getStats">Get stats</button>
    </div>
    <div id="left">
        <img id="bestImages" v-for="photo in photos" :src="photo"/>
    </div>
    <div id="right">
    <table id="statsTable" style="float: right">
        <thead>
            <th>Timestamp</th>
            <th>Temperature</th>
            <th>Brightness1</th>
            <th>Brightness2</th>
            <th>Brightness3</th>
            <th>Brightness4</th>
            <th>MoistureAir</th>
            <th>MoistureGround</th>
            <th>Score</th>
        </thead>
        <tbody>
            <tr v-for="el in stats">
                <td>{{el.timestamp}}</td>
                <td>{{el.temp}}></td>
                <td>{{el.brightness1}}</td>
                <td>{{el.brightness2}}</td>
                <td>{{el.brightness3}}</td>
                <td>{{el.brightness4}}</td>
                <td>{{el.moistureair}}</td>
                <td>{{el.moistureground}}</td>
                <td>{{el.score}}</td>
            </tr>
        </tbody>
    </table>
    </div>
</div>
`,
    data(){
      return {
          photos: [],
          stats: [],
          key: []
      }
    },
methods: {
      getPhoto(){
          let photoData = [];
          axios.get("https://iotgreenhousedata.azurewebsites.net/api/DownloadBest")
              .then((response) => {
                  let dataStats = [];
                  dataStats.push(response.data);
                  this.stats = dataStats[0];
                 response.data.forEach(function(photo){
                     photoData.push("data:image/png;base64," + photo.data);
                 });
              });
          this.photos = photoData;
          // document.getElementById("statsTable").hidden = true;
          // document.getElementById("bestImages").hidden = false;
      },
     getStats() {
          const params = new URLSearchParams(window.location.search);
          const name = params.get("key");

          let plantStats = [];
          axios.get("https://iotgreenhousedata.azurewebsites.net/api/DownloadSensorData?key=" + name)
              .then((response) => {
                  plantStats.push(response.data);
                  this.stats = plantStats;
                  document.getElementById("statsTable").hidden = false;
              }).catch((error)=>{
                  console.log(error.valueOf());
          });

          this.photos = [];
          // document.getElementById("bestImages").hidden = true;
     }
}
});

let app = new Vue({
    el: '#app',
});