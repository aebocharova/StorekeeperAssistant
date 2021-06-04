select wh.name as warehouse_name, n.name as nomenclature_name, mc.count 
from movement_content mc
join movement m on m.id = mc.movement_id
join nomenclature n on n.id = mc.nomenclature_id 
join warehouse wh on (m.from_warehouse_id = wh.id OR m.to_warehouse_id = wh.id )
where m.date_time >= '2021-06-03'
order by warehouse_name, nomenclature_name;

select * from movement_content
select * from movement

select f.warehouse_name, f.nomenclature_name, (t.to_count-f.from_count) as Остаток 

select t.warehouse_name, t.nomenclature_name, sum(count)
from
(
	select wh.name as warehouse_name, n.name as nomenclature_name, -sum(mc.count) as count
from movement_content mc
join movement m on m.id = mc.movement_id
join nomenclature n on n.id = mc.nomenclature_id 
join warehouse wh on (m.from_warehouse_id = wh.id)
where m.date_time >= '2021-06-03'
		group by warehouse_name, nomenclature_name
	
	UNION

select wh.name as warehouse_name, n.name as nomenclature_name, sum(mc.count) as count
from movement_content mc
join movement m on m.id = mc.movement_id
join nomenclature n on n.id = mc.nomenclature_id 
join warehouse wh on (m.to_warehouse_id = wh.id)
where m.date_time >= '2021-06-03'
		group by warehouse_name, nomenclature_name
	) t
	
	where warehouse_name <> 'ExternalWherehouse'
 group by t.warehouse_name, t.nomenclature_name		
order by warehouse_name, nomenclature_name


