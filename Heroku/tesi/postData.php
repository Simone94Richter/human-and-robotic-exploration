<?php

$db_conn = pg_connect("dbname=da46hhlnl2gsi5 host=ec2-54-225-249-161.compute-1.amazonaws.com port
    =5432 user=webojtnmkhwhha password=b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336 sslmode=require");

$_POST = json_decode(file_get_contents('php://input'), true);
$uknown = array();
$goal = array();
$wall = array();
$uknown = $_POST["u"];
$goal = $_POST["g"];
$wall = $_POST["w"];
$map_name = $_POST["mapName"];

$today = getdate();
$d = $today['mday'];
$m = $today['mon'];
$y = $today['year'];
$h = $today['hours'];
$min = $today['minutes'];
$s = $today['seconds'];
$created_at = "$d-$m-$y, $h:$min:$s";
$updated_at = $created_at;

$numb_rows = pg_query($db_conn, "SELECT * FROM robotmap");

if(pg_num_rows($numb_rows) == 0){

    $string_u = "(".$uknown[0].")";
    $string_g = "(".$goal[0].")";
    $string_w = "(".$wall[0].")";
    for($i = 1; $i < count($uknown); $i++){
        $string_u = $string_u.", (".$uknown[$i].")";
    }
    for($i = 1; $i < count($wall); $i++){
        $string_w = $string_w.", (".$wall[$i].")";
    }
    $string_map = "(".$map_name[0].")";
    $query = pg_prepare($db_conn, "insert_query", "INSERT INTO robotmap VALUES ($1, $2, $3, $4, $5, $6, $7)");
    $result = pg_execute($db_conn, "insert_query", array(1, $string_u, $string_g, $string_w, $created_at, $updated_at, $string_map));
    //"INSERT INTO commentsreviews VALUES (1, '$product', '$author', '$content', '$created_at', '$updated_at', '$mapName')"
}
else {
    
    //$highest_id_query = pg_query($db_conn, "SELECT * FROM robotmap");

    $new_id = (int)pg_num_rows($numb_rows) + 1;

    $string_u = "(".$uknown[0].")";
    $string_g = "(".$goal[0].")";
    $string_w = "(".$wall[0].")";
    for($i = 1; $i < count($uknown); $i++){
        $string_u = $string_u.", (".$uknown[$i].")";
    }
    for($i = 1; $i < count($wall); $i++){
        $string_w = $string_w.", (".$wall[$i].")";
    }
    $string_map = "(".$map_name[0].")";

    $query = pg_prepare($db_conn, "insert_query", "INSERT INTO robotmap VALUES ($1, $2, $3, $4, $5, $6, $7)");
    $result = pg_execute($db_conn, "insert_query", array($new_id, $string_u, $string_g, $string_w, $created_at, $updated_at, $string_map));
    //"INSERT INTO commentsreviews VALUES ('$new_id', '$product', '$author', '$content', '$created_at', '$updated_at')"
}

if($result){
    echo("Comment submitted");
} else if(!isset($uknown)){
    echo ("something wrong in uknown");
} else if(!isset($goal)){
    echo("something wrong in goal");
} else if(!isset($wall)){
    echo("something wrong in wall");
} else echo("something wrong");
?>