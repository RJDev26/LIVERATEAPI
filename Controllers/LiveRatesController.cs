using LiveExchangeRatesBackend.IServices;
using LiveExchangeRatesBackend.Models;
using LiveExchangeRatesBackend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace LiveExchangeRatesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveRatesController : ControllerBase
    {
        private ILiveRatesService liveRatesService;
        private static DateTime? _lastUpdatedDateTime = null;
       
        public LiveRatesController(ILiveRatesService liveRatesService)
        {
            this.liveRatesService = liveRatesService;
            
        }


        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetLiveRates(
      [FromQuery] DateTime? lastUpdatedDateTime)
        {
            var data = await liveRatesService
                .GetLiveRates(lastUpdatedDateTime);

            DateTime? latestDateTime = lastUpdatedDateTime;

            if (data != null && data.Count > 0)
            {
                latestDateTime = data.Max(x => x.UpdatedDateTime);
            }

            return Ok(new
            {
                IsSuccess = true,
                LastUpdatedDateTime = latestDateTime,
                Data = data
            });
        }







        //[Route("")]
        //public IActionResult GetLiveRates()
        //{
        //    return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetLiveRates() });
        //}
        [Route("getliveratedatawire")]
        public IActionResult GetLiveRatesDataWire()
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetLiveRatesDataWire() });
        }

        [HttpPut]
        [Route("getliverateparam")]
        public IActionResult GetLiveRatesParam(ScriptParam objScriptParam)
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetLiveRatesParam(objScriptParam) });
        }

        [HttpGet]
        [Route("getexchange")]
        public IActionResult GetExchange()
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetExchange() });
        }

        [HttpGet]
        [Route("getclosing/{exchange}/{fileDate}")]
        public IActionResult GetClosing(string exchange, DateTime fileDate)
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetClosing(exchange, fileDate) });
        }

        [HttpGet]
        [Route("getscript")]
        public IActionResult GetScript()
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetScript() });
        }
        [HttpGet]
        [Route("getscriptlist/{exchange}")]
        public IActionResult GetScriptList(string exchange = "MCX")
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetScriptList(exchange) });
        }
        [HttpGet]
        [Route("getdefaultscriptlist")]
        public IActionResult GetDefaultScriptList()
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetDefaultScriptList() });
        }
        [HttpGet]
        [Route("getcottonbales")]
        public IActionResult GetCottonBales()
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetCottonBales() });
        }

        [HttpGet]
        [Route("getcottonbalessell")]
        public IActionResult GetCottonBalesSell()
        {
            return new JsonResult(new { IsSuccess = true, Data = liveRatesService.GetCottonBalesSell() });
        }

        //[HttpGet]
        //[Route("insertscript/{price}/{change}")]
        //public IActionResult InsertNYScript(double price, double change)
        //{
        //    liveRatesService.InsertUSCotton(price, change);
        //    return new JsonResult(new { IsSuccess = true, Data = "" });
        //}

        [HttpPost]
        [Route("insertscript")]

        async public Task<JsonResult> InsertNYScript([FromBody] List<USScriptVM> lstUSScriptVM)
        {
            int count = 1;
            foreach (var item in lstUSScriptVM)
            {
                //var monthSplit = item.month.Split(' ');
                //int month = DateTime.ParseExact(monthSplit[0], "MMM", CultureInfo.CurrentCulture).Month;
                //var lastDayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, month);
                item.code = count.ToString();

                //if (item.name.Contains("Oil")) { item.code = StaticData.SoyabeanOil; }
                //if (item.name.Contains("Meal")) { item.code = StaticData.SoyabeanMeal; }
                //if (item.name.Contains("Soybeans")) { item.code = StaticData.Soyabean; }
                //if (item.name.Contains("Cotton")) { item.code = StaticData.Cotton; }
                //if (item.name.Contains("Futues")) { item.code = StaticData.Futures; }

                //string date=lastDayOfMonth+monthSplit[0].ToString().ToUpper()+ "20" + monthSplit[1].ToString();
                item.name = item.name.Replace(" ", "").ToUpper();
                // item.name = "FUTCOM  " + item.name + "  " + date;
                item.name = "FUTCOM  " + item.name;
                count = count + 1;

                if (item.name.Contains("'")) { item.name = item.name.Replace("'", " "); }
                if (item.name.Contains("PALMOIL"))
                {
                    item.name = item.name.Replace("PALMOIL", "KLC ");
                }
                if (item.name.Contains("COTTON")) { item.name = item.name.Replace("COTTON#2", "US Cotton "); }
                if (item.name.Contains("SOYBEANOIL")) { item.name = item.name.Replace("SOYBEANOIL", "CBOT Oil "); }
                if (item.name.Contains("SOYBEANMEAL")) { item.name = item.name.Replace("SOYBEANMEAL", "CBOT Meal "); }
                if (item.name.Contains("SOYBEAN")) { item.name = item.name.Replace("SOYBEAN", "CBOT Seed "); }




                if (item.last.Contains("-")) { item.last = item.last.Replace("-", "."); }
                if (item.chg.Contains("-"))
                {
                    string s = item.chg.Substring(0, 1);
                    item.chg = item.chg.Replace("-", ".");
                    item.chg = item.chg.Remove(0, 1);
                    if (s.Contains("-")) { item.chg = s + item.chg; }

                }
                if (item.high.Contains("-")) { item.high = item.high.Replace("-", "."); }
                if (item.low.Contains("-")) { item.low = item.low.Replace("-", "."); }
                try
                {
                    if (item.last.Contains('s')) { item.last= item.last.Replace("s",""); }
                    var checkdata = (Convert.ToDouble(item.last));
                }
                catch (Exception ex)
                {
                    continue;
                }
                try
                {
                    var checkdata = (Convert.ToDouble(item.chg));
                }
                catch (Exception ex)
                {
                    item.chg = "0";
                }


                liveRatesService.InsertUSCotton(Convert.ToDouble(item.last), item.chg, Convert.ToDouble(item.high)
                    , Convert.ToDouble(item.low), item.name, item.code);

            }
            return new JsonResult(new { IsSuccess = true, Data = "" });
        }

        [HttpPost]
        [Route("insertEsignalscript")]

        async public Task<JsonResult> InsertEsignalScript([FromBody] EsignalVM lstUSScriptVM)
        {
            liveRatesService.InsertEsignalLiveRate(Convert.ToDouble(lstUSScriptVM.bid), Convert.ToDouble(lstUSScriptVM.ask),
                Convert.ToDouble(lstUSScriptVM.last)
                , Convert.ToDouble(lstUSScriptVM.volume), lstUSScriptVM.symbol, lstUSScriptVM.datetime, "");

            return new JsonResult(new { IsSuccess = true, Data = "" });
        }
        [HttpPost]
        [Route("insertEsignalRow")]

        async public Task<JsonResult> InsertEsignalRow(string rowString)
        {
            liveRatesService.InsertEsignalLiveRate(0, 0, 0, 0, null, null, Convert.ToString(rowString));

            return new JsonResult(new { IsSuccess = true, Data = "" });
        }

    }
}
