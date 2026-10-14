
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_KeToan1.Controllers
{
    [Authorize(Roles = "Admin,KeToan")]
    [Route("api/QuanLyCongNo")]
    [ApiController]
    public class QuanLyCongNo_Controller : ControllerBase
    {
        private readonly NhaCungCap_BLL _nccBll;
        private readonly KhachHang_BLL _khBll;
        private readonly ThanhToan_BLL _ttBll;
        private readonly HoaDonBan_BLL _hdbBll;

        public QuanLyCongNo_Controller(NhaCungCap_BLL _nccBll, Khm÷ßKh‘éì¶»§q«^tĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚&vWBÖ'––BÖ¶†6††ær"•ĞĞ¢´‡GGvWEĞĞ¢V&Æ–2”7F–öå&W7VÇBvWD'”–D´‚…´g&öÕVW'•Ò7G&–æsòÖ¶‚Ğ¢°Ğ¢–b…fÆ–FFT–çWB†Ö¶‚Â$Ü:2¶Œ:6‚Œ:ær"ÂRÂ÷WB7G&–ær6ÆVäÖ’—2”7F–öå&W7VÇBW'&÷"Ğ¢&WGW&âW'&÷#°Ğ Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒö¶„&ÆÂävWD'”–D´‚†6ÆVäÖ“°Ğ¢f"Æ—7BÒFôF–7F–öæ'”Æ—7B†GB“°Ğ Ğ¢–b†Æ—7Bä6÷VçBÓÒĞ¢&WGW&âæ÷Df÷VæB†æWr²7V66W72ÒfÇ6RÂÖW76vRÒB$¶Œ;FærL:ÆÒFªW’¶Œ:6‚Œ:ær<;2Ü:3¢¶6ÆVäÖÒ"Ò“°Ğ Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂFFÒÆ—7E³ÒÒ“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ¢ĞĞ§ĞĞ Ğ 