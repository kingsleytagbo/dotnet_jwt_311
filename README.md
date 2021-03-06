# Json Web Token (JWT) Authentication using jQuery in .NET Core 3.1

### How to Login / Authenicate in .NET Core using a Json Web Token (JWT) and jQuery
<code>
     <script type="text/javascript">

        $(document).ready(function () {

            // ### TODO: Change this to the url of your api server
            const apiUrl = 'https://localhost:44395';

            const public_key = "d62c03a2-57b6-4e14-8153-d05d3aa9ab10";

            const data = { "UserName": "Kingsley", Password: "..gmail.com", RememberMe: true };

            const login = { auth_site: public_key, login: data, username: data.UserName, password: data.Password, rememberme: data.RememberMe };

            const self = this;

            const getUsers = function (url, jwt) {

                return $.ajax({
                        method: 'post',
                        url: url + "/account/getusers",
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

            const authenticate = function (url, login) {
                return $.ajax({
                    method: 'post',
                    url: url + "/account/login",
                    headers: login,
                    body: JSON.stringify({})
                });
            }

            authenticate(apiUrl, login).done(function (jwt) {
                console.log({ "Authentication done > ": jwt });

                setTimeout(
                    function () {
                        getUsers(apiUrl, jwt);
                    }, 5000);

            }).fail(function (xhr, status, error) {
                console.log({ "Authentication > Error ...": xhr.responseText, status: status, error: error });
            });

        });

    </script>
</code>
