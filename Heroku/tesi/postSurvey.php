<?php

$db_conn = pg_connect("dbname=da46hhlnl2gsi5 host=ec2-54-225-249-161.compute-1.amazonaws.com port
    =5432 user=webojtnmkhwhha password=b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336 sslmode=require");

$_POST = json_decode(file_get_contents('php://input'), true);
$choices = array();
$choices = $_POST["choices"];
$mapname = $_POST["mapname"];
$ip = $_POST["ip"];


$today = getdate();
$d = $today['mday'];
$m = $today['mon'];
$y = $today['year'];
$h = $today['hours'];
$min = $today['minutes'];
$s = $today['seconds'];
$created_at = "$d-$m-$y, $h:$min:$s";
$updated_at = $created_at;

//$numb_rows = pg_query($db_conn, "SELECT * FROM survey");

$id = "$ip $created_at";
$string_first_choice = $choices[0];
$string_second_choice = $choices[1];
$string_third_choice = $choices[2];
$string_fourth_choice = $choices[3];

$query = pg_prepare($db_conn, "insert_query", "INSERT INTO surveyexploration VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)");
$result = pg_execute($db_conn, "insert_query", array($id, $string_first_choice, $string_second_choice, $string_third_choice, $string_fourth_choice, $created_at, $updated_at, $mapname, $ip));

if($result){
    echo("Comment submitted");
} else echo("something wrong");
?>