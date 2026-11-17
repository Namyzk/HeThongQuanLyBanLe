# API Login

Gateway base URL: http://localhost:8080. API đăng nhập công khai; các route khác tùy token/cookie của ứng dụng.

- POST /api/Login/login — nhận JSON body
- POST /api/Login/refresh — không bind JSON body; gửi refresh cookie/token nếu có
- POST /api/Login/logout — không bind JSON body; gửi cookie/token nếu có
- GET /api/Login/get-role?username=testadmin — query username

Body đăng nhập:
{
  "Username": "testadmin",
  "Pass": "Test123!"
}

Dùng thông tin tài khoản thực tế trong database. Sau đăng nhập, gửi Authorization: Bearer <token> tới API cần xác thực. Refresh/logout không cần gửi {}.
