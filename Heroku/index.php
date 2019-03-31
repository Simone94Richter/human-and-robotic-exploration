<!DOCTYPE html>
<head>
<title>Insert data to PostgreSQL with php - creating a simple web application</title>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<style>
li {listt-style: none;}
</style>
</head>
<body>
<h2>Enter information regarding book</h2>
<ul>
<form name="insert" action="insert.php" method="POST" >
<li>Book ID:</li><li><input type="text" name="bookid" /></li>
<li>Book Name:</li><li><input type="text" name="book_name" /></li>
<li>Author:</li><li><input type="text" name="author" /></li>
<li>Publisher:</li><li><input type="text" name="publisher" /></li>
<li>Date of publication:</li><li><input type="text" name="dop" /></li>
<li>Price (USD):</li><li><input type="text" name="price" /></li>
<li><input type="submit" /></li>
</form>
</ul>
</body>
</html>
<?php

echo("Welcome");

$con = "dbname=da46hhlnl2gsi5 host=ec2-54-225-249-161.compute-1.amazonaws.com port
=5432 user=webojtnmkhwhha password=b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336 sslmode=require";

$db_url = "postgres://webojtnmkhwhha:b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336@ec2-
54-225-249-161.compute-1.amazonaws.com:5432/da46hhlnl2gsi5"; //lo tengo qui per magari utilità futura

$db_conn = pg_connect("dbname=da46hhlnl2gsi5 host=ec2-54-225-249-161.compute-1.amazonaws.com port
=5432 user=webojtnmkhwhha password=b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336 sslmode=require");

if(!$db_conn){
    echo("Database not reacheable");
}else echo ("Database linked");

phpinfo();

?>