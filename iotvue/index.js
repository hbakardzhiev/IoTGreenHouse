Vue.component('axios-requests', {
  template: `
<div>
    <div>
        <button @click="getPhoto">Get best performing</button>
        <button @click="getStats">Get stats</button>
    </div>
    <img id="bestImages" v-for="photo in photos" :src="photo"/>
    <table id="statsTable" hidden="hidden">
        <thead>
            <th>Timestamp</th>
            <th>Temperature</th>
        </thead>
        <tbody>
            <tr v-for="el in stats">
                <td>{{el.timestamp.split("T")[0]}}</td>
                <td>{{el.temp}}></td>
            </tr>
        </tbody>
    </table>
</div>
`,
    data(){
      return {
          photos: [],
          stats: []
      }
    },
methods: {
      getPhoto(){
          let photoData = [];
          axios.get("https://iotgreenhousedata.azurewebsites.net/api/DownloadBest")
              .then((response) => {
                 response.data.forEach(function(photo){
                     photoData.push("data:image/png;base64," + photo.data);
                 });
              });
          this.photos = photoData;
          // document.getElementById("statsTable").hidden = true;
          // document.getElementById("bestImages").hidden = false;
      },
     getStats() {
          let plantStats = [];//https://iotgreenhousedata.azurewebsites.net/api/DownloadSensorData
          axios.get("https://iotgreenhousedata.azurewebsites.net/api/DownloadSensorData?key="+"3294202")
              .then((response) => {
                  plantStats.push(response.data);
                  this.stats = plantStats;
                  document.getElementById("statsTable").hidden = false;
              }).catch((error)=>{
                  console.log(error.valueOf());
          });

          // document.getElementById("bestImages").hidden = true;
     }
}
});

let app = new Vue({
    el: '#app',
});