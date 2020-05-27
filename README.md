# dotnet_jwt_311
Json Web Token (JWT) Authentication in .NET Core 3.1

### How to call .NET Core JWT using jQuery
<code>
    <script type="text/javascript">

        $(document).ready(function () {

            const public_key = "d62c03a2-57b6-4e14-8153-d05d3aa9ab10";

            const data = { "UserName": "Kingsley", Password: "..gmail.com", RememberMe: true };

            const login = { auth_site: public_key, login: data, username: data.UserName, password: data.Password, rememberme: data.RememberMe };

            const self = this;

            self.getUsers = function (jwt) {
                $.ajax({
                    method: 'post',
                    url: "https://localhost:44395/account/getusers",
                    body: JSON.stringify({}),
                    headers: {
                        'Authorization': 'Bearer ' + jwt.token
                    }
                })
                    .done(function (users) {
                        console.log({ "getUsers > Success ...": users });
                    })
                    .fail(function (xhr, status, error) {
                        console.log({ "getUsers > Error ...": xhr.responseText, status: status, error: error });
                    });
            }

            const authenticate =
                $.ajax({
                    method: 'post',
                    url: "https://localhost:44395/account/login", //"https://localhost:44374/api/values/login",
                    headers: login,
                    body: JSON.stringify({})
                });

            authenticate.done(function (jwt) {
                console.log({ "Authentication done > ": jwt });

                setTimeout(
                    function () {
                        self.getUsers(jwt);
                    }, 5000);

            }).fail(function (xhr, status, error) {
                console.log({ "Authentication > Error ...": xhr.responseText, status: status, error: error });
            });

        });

    </script>
</code>
