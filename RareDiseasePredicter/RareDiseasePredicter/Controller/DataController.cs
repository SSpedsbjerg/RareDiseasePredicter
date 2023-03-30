using RareDiseasePredicter.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Net;
using System.Linq;

namespace RareDiseasePredicter.Controller {
    public class DataController : ApiController {
        static readonly IDeterminer determiner = null;
        HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost:8080");

        config.Routes.MapHttpRoute(

            );

        [HttpPost]
        public async Task<ActionResult<ISymptom>> PostSymptoms(ISymptom symptom) {
            return CreatedAtActionResult(symptom.GetName(), new {id = symptom.GetID()}, symptom);
        }
    }
}
