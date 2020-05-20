# dotnet_jwt_311
Json Web Token (JWT) Authentication in .NET Core 3.1

### How to call .NET Core JWT using jQuery
<code>
    <script type="text/javascript">
        $(document).ready(function () {
            const data = { "login": "Kingsley" };

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
                        console.log({ "success ...": users });
                    })
                    .fail(function (xhr, status, error) {
                        console.log({ "getUsers error ...": xhr.responseText, status: status, error: error });
                    });
            }

            const authenticate =
                $.ajax({
                    method: 'post',
                    url: "https://localhost:44395/account/login", //"https://localhost:44374/api/values/login",
                    headers: { "login": "kingsley" ,  auth_site: "d62c03a2-57b6-4e14-8153-d05d3aa9ab10" },
                    body: JSON.stringify(data)
                });

            authenticate.done(function (jwt) {
                console.log({ "authentication done ...": jwt });

                setTimeout(
                    function () {
                        self.getUsers(jwt);
                    }, 5000);

            }).fail(function (msg) {
                console.log(msg);
            });

        });
    </script>
</code>
