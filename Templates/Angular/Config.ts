//#region ODataApi Imports
{% for import in Imports %}import { {{import.Names | join: ", "}} } from '{{import.Path}}';
{% endfor %}//#endregion

export const {{Name}} = {
  serviceRootUrl: '{{Package.ServiceRootUrl}}',
  creation: new Date('{{Package.Creation | date: "o"}}'),
  schemas: {
    {% for schema in Package.Schemas %}'{{schema.Namespace}}': {{schema.Name}}{% unless forloop.last %},
    {% endunless %}{% endfor %}}
}