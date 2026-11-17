FROM httpd:alpine
RUN sed -i \
    -e 's/^#LoadModule proxy_module /LoadModule proxy_module /' \
    -e 's/^#LoadModule proxy_http_module /LoadModule proxy_http_module /' \
    /usr/local/apache2/conf/httpd.conf \
    && echo 'Include conf/extra/httpd-proxy.conf' >> /usr/local/apache2/conf/httpd.conf
COPY httpd.conf /usr/local/apache2/conf/extra/httpd-proxy.conf
COPY . /usr/local/apache2/htdocs/
EXPOSE 80
