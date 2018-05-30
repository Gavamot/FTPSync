using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Entety
{
    /// <summary>
    /// Статус выполнения операции обновления записи в БД
    /// </summary>
    public enum UpdateEntetyStatus
    {
        /// <summary>
        /// Не существует в бд
        /// </summary>
        notExist = 1,
        /// <summary>
        /// Значение уже установленно
        /// </summary>
        notUpdate = 2,
        /// <summary>
        /// Запись обновленна
        /// </summary>
        updated = 3
    }
}
