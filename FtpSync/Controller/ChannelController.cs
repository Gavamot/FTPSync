﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FtpSync.Controller
{
    public class ChannelController : MyController
    {
        [HttpPost]
        public IHttpActionResult SyncByPeriod([FromBody] VideoIntervalModel model)//(int brigadeCode, string start, string end)
        {
            // Поиск видеорегистратора в базе
            var reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == model.BrigadeCode);
            if (reg == null)
            {
                return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
            }
            // Выполнение операции
            if (ChannelTaskManager.Instance.SyncChannelsByPeriod(reg, model.Interval))
                return Ok();
            return BadRequest($"{model.BrigadeCode}({model.Interval}) - [ALREADY EXECUTE]");
        }


    }

}
