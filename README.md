# TallerApi

API del taller mecánico Andes Motor. Registra a los clientes y las órdenes de servicio de sus vehículos.

El taller anotaba cada ingreso en un cuaderno. Desde ahí no se podía consultar qué trabajos seguían abiertos, ni evitar que una orden ya entregada se modificara, ni impedir borrar a un cliente que todavía tenía trabajos a su nombre.

## Datos

Un cliente tiene muchas órdenes. La orden guarda la clave `ClienteId` y una navegación `Cliente` opcional, así un alta envía solo la clave.

La orden queda en `Recibida`, `EnReparacion` o `Entregada`.

## Reglas

- El documento del cliente no se repite.
- El número de la orden no se repite.
- No se elimina un cliente que tiene órdenes.
- Una orden entregada no se modifica ni se elimina.

Si una regla no se cumple, la respuesta es 409.

## Conexión

La cadena `Taller` no está en el repositorio. En local se guarda con user-secrets. En el servidor, con la variable `ConnectionStrings__Taller`.

El plan gratuito usable no ofrece SQL Server, así que la base es PostgreSQL y Entity Framework Core entra por Npgsql (`UseNpgsql`). `TallerDB.sql` crea las tablas y los datos de ejemplo. CORS acepta únicamente el origen `Cors:Origin` (`http://localhost:5173` si no se cambia).

Swagger queda publicado en `/swagger`.

## Publicación

La API y la base van en [Render](https://render.com), plan gratuito, sin tarjeta. El servicio web construye el `Dockerfile` y queda en HTTPS. PostgreSQL está en el mismo proveedor, así la API no depende de una base local. La cadena llega por la variable `ConnectionStrings__Taller` y no se guarda en el repositorio. La base gratuita de Render caduca a los 30 días; alcanza para la revisión del trabajo. El archivo `render.yaml` describe los dos servicios.
