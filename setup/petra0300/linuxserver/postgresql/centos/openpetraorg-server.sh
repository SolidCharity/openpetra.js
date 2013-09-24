#!/bin/sh
#
# chkconfig: 345 96 24
# description: Starts and stops the openpetraorg server running with Mono
#

# Find the name of the script
NAME=`basename $0`
if [ ${NAME:0:1} = "S" -o ${NAME:0:1} = "K" ]
then
        NAME=${NAME:3}
fi

if [ -z "$OpenPetraOrgPath" ]
then
  export OpenPetraOrgPath=/usr/local/openpetraorg
  export userName=openpetra
  export OPENPETRA_RDBMSType=postgresql
  export OPENPETRA_DBHOST=localhost
  export OPENPETRA_DBPWD=@RandomDBPassword@
  export OPENPETRA_DBUSER=petraserver
  export OPENPETRA_DBNAME=openpetra
  export OPENPETRA_DBPORT=5432
  export OPENPETRA_PORT=@HostedPort@
  export POSTGRESQLVERSION=@PostgreSQL.Version@
  export backupfile=$OpenPetraOrgPath/backup30/backup-`date +%Y%m%d`.sql.gz
  export mono_path=/opt/mono-openpetra
  export FASTCGI_MONO_SERVER=$mono_path/bin/fastcgi-mono-server4
  export mono=$mono_path/bin/mono
fi

# Override defaults from /etc/sysconfig/openpetra if file is present
[ -f /etc/sysconfig/openpetra/${NAME} ] && . /etc/sysconfig/openpetra/${NAME}

if [ "$2" != "" ]
then
  backupfile=$2
  useremail=$2
  ymlgzfile=$2
fi

. /lib/lsb/init-functions

log_daemon_msg() { logger "$@"; echo "$@"; }
log_end_msg() { [ $1 -eq 0 ] && RES=OK; logger ${RES:=FAIL}; }

# start the openpetraorg server
start() {
    log_daemon_msg "Starting OpenPetra.org server"

    su $userName -c "PATH=$mono_path/bin:$PATH $FASTCGI_MONO_SERVER /socket=tcp:127.0.0.1:$OPENPETRA_PORT /applications=/:/var/www/html /appconfigfile=/home/$userName/etc/PetraServerConsole.config&"
    status=0
    log_end_msg $status
}

# stop the openpetraorg server
stop() {
    log_daemon_msg "Stopping OpenPetra.org server"
    cd $OpenPetraOrgPath/bin30
    
    su $userName -c "$mono --runtime=v4.0 --server PetraServerAdminConsole.exe -C:/home/$userName/etc/PetraServerAdminConsole.config -Command:Stop"
    
    status=0
    log_end_msg $status
}

# load a new database from a yml.gz file. this will overwrite the current database!
loadYmlGz() {
    cd $OpenPetraOrgPath/bin30
    su $userName -c "$mono --runtime=v4.0 --server PetraServerAdminConsole.exe -C:/home/$userName/etc/PetraServerAdminConsole.config -Command:LoadYmlGz -YmlGzFile:$ymlgzfile"
    status=0
    log_end_msg $status
}

# display a menu to check for logged in users etc
menu() {
    cd $OpenPetraOrgPath/bin30
    su $userName -c "$mono --runtime=v4.0 --server PetraServerAdminConsole.exe -C:/home/$userName/etc/PetraServerAdminConsole.config"
}

# backup the postgresql database
backup() {
    echo `date` "Writing to " $backupfile
    # loading of this dump will show errors about existing data tables etc.
    # could have 2 calls: --data-only and --schema-only.
    su $userName -c "pg_dump -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME | gzip > $backupfile"
    echo `date` "Finished!"
}

# restore the postgresql database
restore() {
    echo "This will overwrite your database!!!"
    echo "Please enter 'yes' if that is ok:"
    read response
    if [ "$response" != 'yes' ]
    then
        echo "Cancelled the restore"
        exit
    fi
    echo `date` "Start restoring from " $backupfile
    echo "dropping tables and sequences..."

    delCommand="SELECT 'DROP TABLE ' || n.nspname || '.' || c.relname || ' CASCADE;' FROM pg_catalog.pg_class AS c LEFT JOIN pg_catalog.pg_namespace AS n ON n.oid = c.relnamespace WHERE relkind = 'r' AND n.nspname NOT IN ('pg_catalog', 'pg_toast') AND pg_catalog.pg_table_is_visible(c.oid)" 
    su $userName -c "psql -t -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -c \"$delCommand\" > /tmp/deleteAllTables.sql"

    delCommand="SELECT 'DROP SEQUENCE ' || n.nspname || '.' || c.relname || ';' FROM pg_catalog.pg_class AS c LEFT JOIN pg_catalog.pg_namespace AS n ON n.oid = c.relnamespace WHERE relkind = 'S' AND n.nspname NOT IN ('pg_catalog', 'pg_toast') AND pg_catalog.pg_table_is_visible(c.oid)" 
    su $userName -c "psql -t -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -c \"$delCommand\" > /tmp/deleteAllSequences.sql"
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f /tmp/deleteAllTables.sql"
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f /tmp/deleteAllSequences.sql"

    rm /tmp/deleteAllTables.sql
    rm /tmp/deleteAllSequences.sql

    export PGOPTIONS='--client-min-messages=warning'

    echo "creating tables..."
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $OpenPetraOrgPath/db30/createtables-PostgreSQL.sql"

    echo "loading data..."
    echo $backupfile|grep -qE '\.gz$'
    if [ $? -eq 0 ]
    then
        su $userName -c "cat $backupfile | gunzip | psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q > /home/$userName/log/pgload.log"
    else
        su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $backupfile > /home/$userName/log/pgload.log"
    fi

    echo "enabling indexes and constraints..."
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $OpenPetraOrgPath/db30/createconstraints-PostgreSQL.sql"

    echo `date` "Finished!"
}

init() {
    echo "creating database..."
    service postgresql-$POSTGRESQLVERSION initdb
    service postgresql-$POSTGRESQLVERSION start

    echo "local  $OPENPETRA_DBNAME $OPENPETRA_DBUSER   md5" > /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf.new
    echo "host  $OPENPETRA_DBNAME $OPENPETRA_DBUSER  ::1/128   md5" >> /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf.new
    echo "host  $OPENPETRA_DBNAME $OPENPETRA_DBUSER  127.0.0.1/32   md5" >> /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf.new
    cat /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf >> /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf.new
    mv -f /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf.new /var/lib/pgsql/$POSTGRESQLVERSION/data/pg_hba.conf
    /etc/init.d/postgresql-$POSTGRESQLVERSION restart

    su postgres -c "psql -q -p $OPENPETRA_DBPORT -c \"CREATE USER \\\"$OPENPETRA_DBUSER\\\" PASSWORD '$OPENPETRA_DBPWD'\""
    su postgres -c "createdb -p $OPENPETRA_DBPORT -T template0 -O $OPENPETRA_DBUSER $OPENPETRA_DBNAME"

    useradd --home /home/$userName $userName
    mkdir -p /home/$userName/log
    mkdir -p /home/$userName/tmp
    mkdir -p /home/$userName/etc
    hostname=`hostname`
    # copy config files (server, serveradmin.config) to etc, with adjustments
    cat $OpenPetraOrgPath/etc30/PetraServerConsole.config \
       | sed -e "s/OPENPETRA_DBHOST/$OPENPETRA_DBHOST/" \
       | sed -e "s/OPENPETRA_DBUSER/$OPENPETRA_DBUSER/" \
       | sed -e "s/OPENPETRA_DBNAME/$OPENPETRA_DBNAME/" \
       | sed -e "s/OPENPETRA_DBPORT/$OPENPETRA_DBPORT/" \
       | sed -e "s/USERNAME/$userName/" \
       > /home/$userName/etc/PetraServerConsole.config
    cat $OpenPetraOrgPath/etc30/PetraServerAdminConsole.config \
       | sed -e "s/DOMAINNAME/`hostname`/" \
       | sed -e "s/USERNAME/$userName/" \
       | sed -e "s/OPENPETRA_PORT/$OPENPETRA_PORT/" \
       > /home/$userName/etc/PetraServerAdminConsole.config

    echo "*:$OPENPETRA_DBPORT:$OPENPETRA_DBNAME:$OPENPETRA_DBUSER:$OPENPETRA_DBPWD" >> /home/$userName/.pgpass
    chown -R $userName:$userName /home/$userName
    chmod 600 /home/$userName/.pgpass
    chown $userName /home/$userName/.pgpass

    echo "creating tables..."
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $OpenPetraOrgPath/db30/createtables-PostgreSQL.sql"
    echo "enabling indexes and constraints..."
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $OpenPetraOrgPath/db30/createconstraints-PostgreSQL.sql"

    # configure lighttpd
    cat > /etc/lighttpd/vhosts.d/openpetra$OPENPETRA_PORT.conf <<FINISH
\$HTTP["url"] =~ "^/openpetra$OPENPETRA_PORT" {
  var.server_name = "openpetra$OPENPETRA_PORT"

  server.name = "$hostname/openpetra$OPENPETRA_PORT"

  server.document-root = "/var/www/html"

  fastcgi.server = (
        "/openpetra$OPENPETRA_PORT" => ((
                "host" => "127.0.0.1",
                "port" => $OPENPETRA_PORT,
                "check-local" => "disable"
        ))
  )
}
FINISH
    sed -i 's~server.modules = (~server.modules = (\n  "mod_fastcgi",~g' /etc/lighttpd/modules.conf
    sed -i 's/server.use-ipv6 = "enable"/server.use-ipv6 = "disable"/g' /etc/lighttpd/lighttpd.conf
    sed -i 's/server.max-connections = 1024/server.max-connections = 512/g' /etc/lighttpd/lighttpd.conf
    echo 'include_shell "cat /etc/lighttpd/vhosts.d/*.conf"' >> /etc/lighttpd/lighttpd.conf
    service lighttpd restart
    chkconfig lighttpd on
    chkconfig openpetra-server on
    service openpetra-server start
    ymlgzfile=$OpenPetraOrgPath/db30/base.yml.gz
    loadYmlGz
}

case "$1" in
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        stop
        start
        ;;
    backup)
        backup
        ;;
    restore)
        restore
        ;;
    init)
        init
        ;;
    loadYmlGz)
        loadYmlGz
        ;;
    menu)
        menu
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|menu|backup|restore|init|loadYmlGz}"
        exit 1
        ;;
esac

exit 0
