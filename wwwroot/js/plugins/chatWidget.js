(function e(t,n,i){function o(s,r){if(!n[s]){if(!t[s]){var d="function"==typeof require&&require
if(!r&&d)return d(s,!0)
if(a)return a(s,!0)
var u=new Error("Cannot find module '"+s+"'")
throw u.code="MODULE_NOT_FOUND",u}var l=n[s]={exports:{}}
t[s][0].call(l.exports,function(e){var n=t[s][1][e]
return o(n||e)},l,l.exports,e,t,n,i)}return n[s].exports}for(var a="function"==typeof require&&require,s=0;s<i.length;s++)o(i[s])
return o})({1:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=a(e("./../obj/constants")),o=a(e("./../utils/common/dom"))
function a(e){return e&&e.__esModule?e:{default:e}}n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=function(e){27===e.keyCode&&this.destroy()}
return{create:function(t){var n=document,o=n.body,a=n.createElement("DIV"),s=n.createElement("IMG"),r=n.createElement("DIV"),d=n.createElement("DIV")
a.setAttribute("id",i.default.modalDivId),a.onclick=this.destroy,s.setAttribute("src",t),s.onclick=this.destory,d.appendChild(s),d.className="image-cell",r.appendChild(d),r.className="image-wrapper",a.appendChild(r),o.appendChild(a),window.addEventListener?window.addEventListener("keydown",e.bind(this),!1):window.attachEvent("keydown",e.bind(this),!1)},destroy:function(){var t=document,n=t.body,a=t.getElementById(i.default.modalDivId)
a&&(o.default.purge(a),n.removeChild(a)),window.removeEventListener?window.removeEventListener("keydown",e.bind(this)):window.detachEvent("keydown",e.bind(this))}}}.bind(void 0)()},{"./../obj/constants":7,"./../utils/common/dom":12}],2:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=a(e("./omni/transition")),o=a(e("./omni/activity"))
function a(e){return e&&e.__esModule?e:{default:e}}n.default={init:function(){i.default.start(),o.default.start()},destroy:function(){i.default.stop(),o.default.stop()}}},{"./omni/activity":3,"./omni/transition":5}],3:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=a(e("./../../utils/frame")),o=a(e("./../../utils/common/dom"))
function a(e){return e&&e.__esModule?e:{default:e}}function s(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")}n.default=function(){s(void 0,void 0)
var e=function(){s(void 0,void 0),i.default.postMessage({action:"track_activity",payload:{seen:n}})}.bind(void 0),t=null,n=null,a=null,r=function(){s(void 0,void 0),n=(new Date).getTime()}.bind(void 0),d=function(e){s(void 0,void 0),o.default.unbindEvent("keypress",r),o.default.unbindEvent("mousemove",r),o.default.unbindEvent("click",r),e&&(o.default.bindEvent("keypress",r),o.default.bindEvent("mousemove",r),o.default.bindEvent("click",r))}.bind(void 0),u=function(){s(void 0,void 0)
var i=(new Date).getTime()
null===a||null===n||t&&(i<t||n<t||i-t<59e3||n>t&&n<i&&e()),t=i}.bind(void 0),l=function(){s(void 0,void 0),a&&clearInterval(a),a=null,n=null,t=null}.bind(void 0),f=function(){s(void 0,void 0),l(),d(!0),t=(new Date).getTime(),a=setInterval(u,6e4)}.bind(void 0),c=function(){s(void 0,void 0),n=(new Date).getTime(),e(),l(),d(!1)}.bind(void 0),m=function(){s(void 0,void 0),"hidden"===document.visibilityState&&c()}.bind(void 0)
return{start:function(){f(),o.default.bindEvent("focus",f),o.default.bindEvent("blur",c),o.default.bindEvent("beforeunload",c),o.default.bindEvent("visibilitychange",m)},stop:function(){c(),o.default.unbindEvent("focus",f),o.default.unbindEvent("blur",c),o.default.unbindEvent("beforeunload",c),o.default.unbindEvent("visibilitychange",m)}}}.bind(void 0)()},{"./../../utils/common/dom":12,"./../../utils/frame":16}],4:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./../../utils/msg_handler"),a=(i=o)&&i.__esModule?i:{default:i}
n.default={track:function(e,t){a.default.subscribe("track_event",{eventName:e,data:t})}}},{"./../../utils/msg_handler":19}],5:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=a(e("./../../utils/common/observe")),o=a(e("./../../utils/frame"))
function a(e){return e&&e.__esModule?e:{default:e}}n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=void 0,t=void 0,n=void 0,a=function(i){e=i.oldValue,t=i.newValue,n=i.title,o.default.postMessage({action:"track_location",payload:{pageUrl:i}})},s=function(){var i=window.location.href
t!==i&&(e=t,t=i,n=document.title,a({oldValue:e,newValue:t,title:n}))}
return{start:function(e){t=window.location.href,a({newValue:t,title:document.title}),i.default.attach(window.history,"pushState",s),i.default.attach(window.history,"replaceState",s),window.addEventListener?(window.addEventListener("hashchange",s,!1),window.addEventListener("popstate",s,!1)):(window.attachEvent("hashchange",s,!1),window.attachEvent("popstate",s,!1))},stop:function(){e=t=void 0,i.default.detach(window.history,"pushState"),i.default.detach(window.history,"replaceState"),window.removeEventListener?(window.removeEventListener("hashchange",s),window.removeEventListener("popstate",s)):(window.detachEvent("hashchange",s),window.detachEvent("popstate",s))},track:a}}.bind(void 0)()},{"./../../utils/common/observe":14,"./../../utils/frame":16}],6:[function(e,t,n){"use strict"
var i,o,a=e("./widget"),s=(i=a)&&i.__esModule?i:{default:i}
if(window.fcWidget||(window.fcWidget=s.default),window.fcSettings)if("complete"===document.readyState){var r=window.fcSettings.onInit
r&&"function"==typeof r&&r(),s.default.init(window.fcSettings)}else document.onreadystatechange=(o=s.default,function(){if("complete"===document.readyState){var e=window.fcSettings.onInit
e&&"function"==typeof e&&e(),o.init(window.fcSettings)}})},{"./widget":24}],7:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0}),n.default={frameDivId:"fc_frame",frameId:"fc_widget",pushFrameDivId:"fc_push_frame",pushFrameId:"fc_push",modalDivId:"fc_web_modal",classes:{fullscreenClass:"fc-widget-fullscreen"}}},{}],8:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0}),n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=!1,t=!1,n=!1,i=!1,o=[],a={}
return{isLoaded:function(){return e},loaded:function(t){e=t},isInitialized:function(){return t},initialized:function(e){t=e},isOpened:function(){return n},opened:function(e){n=e},doOpen:function(){return i},openOnLoad:function(e){i=e},getTags:function(){return o},setTags:function(e){o=e},getFaqTags:function(){return a},setFaqTags:function(e){a=e},reset:function(){e=!1,t=!1,n=!1,i=!1,o=[],a={}}}}.bind(void 0)()},{}],9:[function(e,t,n){"use strict"
function i(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")}Object.defineProperty(n,"__esModule",{value:!0}),n.default=function(){i(void 0,void 0)
var e=null,t=null,n=!1,o={},a={},s=["firstName","lastName","email","phone","phoneCountry","locale"]
return{getExternalId:function(){return e},setExternalId:function(t){e=t},getRestoreId:function(){return t},setRestoreId:function(e){t=e},setIdentifyByReferenceId:function(e){n=e},getIdentifyByReferenceId:function(){return n},getConfig:function(){return o},setConfig:function(e){o=e},getProperties:function(){return a},setProperties:function(e){var t=this
e&&(s.forEach(function(n){i(this,t),e[n]&&(a[n]=e[n])}.bind(this)),e.meta&&this.setUserMeta(e.meta))},setFirstName:function(e){a.firstName=e},setLastName:function(e){a.lastName=e},setEmail:function(e){a.email=e},setPhone:function(e){a.phone=e},setPhoneCountry:function(e){a.phoneCountry=e},setUserMeta:function(e){if(a.meta=a.meta||{},e)for(var t in e)e.hasOwnProperty(t)&&(a.meta[t]=e[t])},setLocale:function(e){a.locale=e},reset:function(){e=null,t=null,o={},a={}},getJSON:function(){return{externalId:e,restoreId:t,customConfig:o,properties:a}}}}.bind(void 0)()},{}],10:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./common/dom"),a=(i=o)&&i.__esModule?i:{default:i}
n.default=function(){return function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")}(void 0,void 0),{init:function(e){var t=document.createElement("script"),n=a.default.cdn_url()?a.default.cdn_url():e.host
t.type="text/javascript",t.src=n+"/js/cb.js",(document.body?document.body:document.getElementsByTagName("head")[0]).appendChild(t),window.fc_cobrowse={host:e.host,locale:e.locale}}}}.bind(void 0)()},{"./common/dom":12}],11:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./../frame"),a=(i=o)&&i.__esModule?i:{default:i}
n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=function(){return a.default.getHost()+url.replace("{token}",a.default.getToken())},t=function(e){if(e){void 0===e.async&&(e.async=!0)
var t=function(){if("XMLHttpRequest"in window)return new XMLHttpRequest
for(var e,t=["MSXML2.XmlHttp.6.0","MSXML2.XmlHttp.5.0","MSXML2.XmlHttp.4.0","MSXML2.XmlHttp.3.0","MSXML2.XmlHttp.2.0","Microsoft.XmlHttp"],n=0;n<t.length;n++)try{e=new ActiveXObject(t[n])
break}catch(e){}return e}()
t.open(e.type,e.url,e.async),t.onreadystatechange=function(){4===t.readyState&&e.success(JSON.parse(t.responseText))},"GET"!==e.type&&(e.contentType?t.setRequestHeader("Content-Type",e.contentType):t.setRequestHeader("Content-type","application/x-www-form-urlencoded")),e.data?t.send(e.data):t.send()}}
return{getRequest:function(n,i){t({url:e(),type:"GET",success:i})},putRequest:function(n,i,o){t({url:e(),type:"PUT",data:JSON.stringify(i),contentType:"application/json; charset=UTF-8",success:o})}}}.bind(void 0)()},{"./../frame":16}],12:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o,a,s,r,d,u,l,f,c,m=e("../../../config/cb_environment"),p=(i=m)&&i.__esModule?i:{default:i}
n.default={url_domain:function(e){var t=document.createElement("a")
return t.href=e,t.origin},getElementStyle:function(e,t){return window.getComputedStyle(e)[t]},setAttr:function(e,t){for(var n in t)t.hasOwnProperty(n)&&e.setAttribute(n,t[n])
return e},remove:function(e){var t=document,n=t.body,i=t.getElementById(e)
i&&(this.purge(i),n.removeChild(i))},purge:function(e){var t,n,i,o=e.attributes
if(o)for(t=o.length-1;t>=0;t-=1)"function"==typeof e[i=o[t].name]&&(e[i]=null)
if(o=e.childNodes)for(n=o.length,t=0;t<n;t+=1)this.purge(e.childNodes[t])},cdn_url:function(){if(p.default&&p.default.config){var e=p.default.config.type,t=p.default.config.cdn
return t.protocol[e]+(t.enabled.forAssets?t.assets:"")+t.domain[e]}},bindEvent:function(e,t){var n=arguments.length>2&&void 0!==arguments[2]&&arguments[2]
window.addEventListener?window.addEventListener(e,t,n):window.attachEvent(e,t,n)},unbindEvent:function(e,t){var n=arguments.length>2&&void 0!==arguments[2]&&arguments[2]
window.removeEventListener?window.removeEventListener(e,t,n):window.detachEvent(e,t,n)},parseJSON:function(e){var t
if(e){try{t=JSON.parse(e)}catch(e){}return t}},storageAvailable:function(e){try{var t=window.localStorage,n="__storage_test__"
return t.setItem(n,n),t.removeItem(n),!0}catch(e){return e instanceof DOMException&&(22===e.code||1014===e.code||"QuotaExceededError"===e.name||"NS_ERROR_DOM_QUOTA_REACHED"===e.name)&&0!==t.length}}(),isPushSupportedByBrowser:function(){var e,t,n,i=!1,o=!1,a=function(){var e=!1
try{ServiceWorkerRegistration&&"showNotification"in ServiceWorkerRegistration.prototype&&(e=!0)}catch(e){}return e};(function(){var e=window.chrome,t=window.navigator,n=t.vendor,i=t.userAgent.indexOf("OPR")>-1,o=t.userAgent.indexOf("Edge")>-1
t.userAgent.match("CriOS")
return null!==e&&void 0!==e&&"Google Inc."===n&&!1===i&&!1===o})()&&(e=!!(n=navigator.userAgent.match(/Chrom(e|ium)\/([0-9]+)\./))&&parseInt(n[2],10))&&(i=e>=50),navigator.userAgent.toLowerCase().indexOf("firefox")>-1&&(t=function(){var e=navigator.userAgent,t=e.indexOf("Firefox"),n=e.substring(t+8).split(".")
return!(!n||!n.length)&&parseInt(n[0],10)}())&&(o=t>=44)
var s="serviceWorker"in navigator,r=(a=a(),"PushManager"in window)
return s&&a&&r&&(i||o)},getAgent:(d=navigator.appVersion,u=navigator.userAgent,l=navigator.appName,f=""+parseFloat(d),c=parseInt(d,10),-1!==u.indexOf("Station")&&(r="Station"),-1!==(a=u.indexOf("Opera"))?(l="Opera",f=u.substring(a+6),-1!==(a=u.indexOf("Version"))&&(f=u.substring(a+8))):-1!==(a=u.indexOf("MSIE"))?(l="Microsoft Internet Explorer",f=u.substring(a+5)):-1!==(a=u.indexOf("Edge"))?(l="Edge",f=u.substring(a+5)):-1!==(a=u.indexOf("Trident"))?(l="Trident",f=u.substring(a+8)):-1!==(a=u.indexOf("Chrome"))?(l="Chrome",f=u.substring(a+7)):-1!==(a=u.indexOf("Safari"))?(l="Safari",f=u.substring(a+7),-1!==(a=u.indexOf("Version"))&&(f=u.substring(a+8))):-1!==(a=u.indexOf("Firefox"))?(l="Firefox",f=u.substring(a+8)):-1===(a=u.indexOf("Mobile"))||-1===u.indexOf("iPad")&&-1===u.indexOf("iPhone")&&-1===u.indexOf("iPod")&&-1===u.indexOf("wv")?(o=u.lastIndexOf(" ")+1)<(a=u.lastIndexOf("/"))&&(l=u.substring(o,a),f=u.substring(a+1),l.toLowerCase()===l.toUpperCase()&&(l=navigator.appName)):(l="WebView",f=u.substring(a+8)),-1!==(s=f.indexOf(";"))&&(f=f.substring(0,s)),-1!==(s=f.indexOf(" "))&&(f=f.substring(0,s)),c=parseInt(""+f,10),isNaN(c)&&(f=""+parseFloat(d),c=parseInt(d,10)),{name:l,appName:r,version:c,versionx:f,os:navigator.platform})}},{"../../../config/cb_environment":25}],13:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0}),n.default={isFunction:function(e){return!(!e||"function"!=typeof e)}}},{}],14:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0}),n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e={}
return{attach:function(t,n,i){var o=n+"_fc_observer"
t[n]&&i&&(void 0===t[o]||t[o]!==i)&&(t[o]=i,function(t,n){e[n]=t[n],t[n]=function(){var i=e[n].apply(t,arguments),o=t[n+"_fc_observer"]
return"function"==typeof o&&o(),i}}(t,n))},detach:function(t,n){var i=n+"_fc_observer"
t[n]&&t[i]&&(t[i]=void 0,function(t,n){e[n]&&(delete t[n],t[n]=e[n],delete e[n])}(t,n))}}}.bind(void 0)()},{}],15:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./common/js_util"),a=(i=o)&&i.__esModule?i:{default:i}
n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e={},t=["widget:opened","widget:closed","widget:loaded","dialog:opened","dialog:closed","widget:destroyed","frame:statechange","user:statechange","user:created","user:cleared","user:authenticated","message:sent","message:received","unreadCount:notify","push:subscribed"]
return{clear:function(){e={}},valid:function(e){return!(!e||!t)&&-1!==t.indexOf(e)},subscribe:function(t,n){this.valid(t)&&(void 0===e[t]&&(e[t]=[]),e[t].push(n))},unsubscribe:function(t,n){if(this.valid(t)){var i,o=e&&e[t]
if(o)for(var s=0,r=o.length;s<r;s++)if(i=o[s],a.default.isFunction(i)&&(!n||i.name===n.name)){e[t].splice(s,1)
break}}},publish:function(t,n,i){if(this.valid(t)){var o,a=e&&e[t]
if(a)for(var s=0,r=a.length;s<r;s++){o=a[s]
var d=function(){n?o(n):o()}
i?setTimeout(d,0):d()}}}}}.bind(void 0)()},{"./common/js_util":13}],16:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=s(e("./../obj/constants")),o=s(e("./../obj/user")),a=s(e("./common/dom"))
function s(e){return e&&e.__esModule?e:{default:e}}n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=null,t=null,n=null,s=null,r=null,d=null,u=!1,l=!1,f=null,c=!1,m=["get_user_uuid"]
return{getHost:function(){return r},getToken:function(){return t},getReferrer:function(){return n},getSiteId:function(){return s},getSettings:function(){return d},setSettings:function(e){u=!!((d=e).userAuthConfig&&d.userAuthConfig.jwtAuthEnabled&&d.userAuthConfig.strictModeEnabled),l=!(!d.userAuthConfig||!d.userAuthConfig.jwtAuthEnabled)},isJWTStrictMode:function(){return u},isJWTEnabled:function(){return l},getJWTAuthToken:function(){return f},setJWTAuthToken:function(e){f=e},isLoaded:function(){return c},loaded:function(e){c=e},loadingActions:function(){return m},init:function(e){t=e.token,n=e.referrer,r=e.host,s=e.siteId,u=!1,l=!1,f=e.jwtAuthToken,c=!1},reset:function(){t=n=r=s=u=l=f=c=void 0},getJSON:function(){return{token:t,referrer:n,host:r,siteId:s,jwtStrictMode:u,jwtEnabled:l,jwtAuthToken:f,loaded:c}},load:function(t,n){e=window.open(t,n)},postMessage:function(t){e&&e.postMessage(t,r)},dispatch:function(e,t){e&&t&&"function"==typeof e&&e(t)},unload:function(){this.postMessage({action:"push_subscribe_destroy"})},add:function(){var e=document,s=e.body,d=e.getElementById(i.default.frameDivId),u=e.createElement("IFRAME"),l=o.default.getConfig(),f=l&&l.cssNames
d&&"DIV"===d.tagName||(d=e.createElement("DIV"),s.appendChild(d)),a.default.setAttr(d,{id:i.default.frameDivId,class:f&&f.widget||""}),d.classList.add("fc_dn"),l&&l.headerProperty?("ltr"===l.headerProperty.direction&&d.classList.add("fc_l2r"),l.headerProperty.hideChatButton||d.classList.remove("fc_dn")):d.classList.remove("fc_dn"),a.default.setAttr(u,{id:i.default.frameId,name:i.default.frameId,title:"Chat",frameborder:"0"}),d.appendChild(u),this.load(r+"/widget/?token="+t+"&referrer="+n,i.default.frameId)},remove:function(t){a.default.remove(t),e=null},setFrameSize:function(){var e=this.getSettings(),t=o.default.getConfig()
if(null===e)return""
var n=e.widgetSize,a=!(!t||!t.fullscreen)&&t.fullscreen,s="fc-widget-normal",r=document.getElementById(i.default.frameDivId)
!0===a&&r.classList.add(i.default.classes.fullscreenClass),n&&n.length>0&&(s="fc-widget-"+n.toLowerCase()),r.classList.add(s)},setWidgetDisplayStyle:function(){var e=document.body,t=document.getElementById(i.default.frameDivId),n=a.default.getElementStyle(e,"display")
n&&-1!==["flex","grid"].indexOf(n)&&a.default.setAttr(t,{style:"display: "+n})}}}.bind(void 0)()},{"./../obj/constants":7,"./../obj/user":9,"./common/dom":12}],17:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=h(e("./common/dom")),a=h(e("./frame")),s=h(e("./events")),r=(h(e("./common/ajax")),h(e("../obj/constants"))),d=h(e("../obj/state")),u=h(e("../obj/user")),l=h(e("./msg_handler")),f=h(e("./push_frame")),c=h(e("../helpers/modal")),m=h(e("../helpers/omni")),p=h(e("./msg_processor")),g=h(e("./cb"))
function h(e){return e&&e.__esModule?e:{default:e}}n.default={onMessageCB:function(){return i||(i=this.onMessage.bind(this))},onVisibilityChangeCB:function(){var e
return function(){return e||(e=this.onVisibilityChange.bind(this))}}(),subscribe:function(){window.addEventListener?(window.addEventListener("message",this.onMessageCB(),!1),window.addEventListener("focus",this.onVisibilityChangeCB(),!1),window.addEventListener("blur",this.onVisibilityChangeCB(),!1)):(window.attachEvent("onmessage",this.onMessageCB()),window.attachEvent("focus",this.onVisibilityChangeCB()),window.attachEvent("blur",this.onVisibilityChangeCB()))},unsubscribe:function(){window.removeEventListener?(window.removeEventListener("message",this.onMessageCB(),!1),window.removeEventListener("focus",this.onVisibilityChangeCB(),!1),window.removeEventListener("blur",this.onVisibilityChangeCB(),!1)):(window.detachEvent("onmessage",this.onMessageCB()),window.detachEvent("focus",this.onVisibilityChangeCB()),window.detachEvent("blur",this.onVisibilityChangeCB()))},onVisibilityChange:function(e){switch(e.type){case"focus":a.default.postMessage({action:"widget_focus"})
break
case"blur":a.default.postMessage({action:"widget_blur",payload:{title:document.title,location:window.location.href}}),f.default.postMessage({action:"widget_location",openWindow:!1,payload:window.location.href})}},onUserCreate:function(e){s.default.publish("user:created",e)},loadWidget:function(){if(!1===d.default.isInitialized()){d.default.initialized(!0)
var e=a.default.getJSON(),t=u.default.getProperties(),n=u.default.getConfig(),i=d.default.getTags(),s=d.default.getFaqTags()
if(e.externalId=u.default.getExternalId(),e.restoreId=u.default.getRestoreId(),e.identifyByReferenceId=u.default.getIdentifyByReferenceId(),t&&(e.properties=t),n&&(e.config=n),i&&(e.tags=i),s&&(e.faqTags=s),e.userAgent=o.default.getAgent,o.default.storageAvailable){var r=e.token,l=localStorage.getItem(r),f=e.siteId?e.token+"_"+e.siteId:null,c=f?localStorage.getItem(f):null;(l||f&&c)&&(e.storage={},e.storage[r]=l||JSON.stringify({}),f&&(e.storage[f]=c||JSON.stringify({})))}a.default.setFrameSize(),a.default.setWidgetDisplayStyle(),a.default.postMessage({action:"load_widget",payload:e})}},unloadWidget:function(){f.default.isLoaded()&&f.default.unload(),d.default.reset(),u.default.reset(),m.default.destroy(),a.default.reset(),a.default.remove(r.default.frameDivId),this.unsubscribe(),s.default.publish("widget:destroyed",null,!0),s.default.clear()},updateFrameSettings:function(e){if(null!==a.default.getSettings()||void 0===e)return!1
a.default.setSettings(e)},onMessage:function(e){var t=e.origin||e.originalEvent.origin,n=u.default.getConfig(),i=n&&n.cssNames
if(t===a.default.getHost()||t===f.default.getHostOrigin()){var h=e.data,v=h&&h.action
if(this.updateFrameSettings(h.settingsPayload),v)switch(v){case"push_subscribe_destroy_response":this.unloadWidget()
break
case"push_user_meta":s.default.publish("push:subscribed",h.data),a.default.postMessage(h)
break
case"toggle_dialog":h.data?s.default.publish("dialog:opened"):s.default.publish("dialog:closed")
break
case"notify_frame":var b=h.data,w=document.getElementById(r.default.frameDivId),_=i&&i.open||"fc-open"
"expand"===b?(w.removeAttribute("style"),w.classList.add("h-open-notify"),w.classList.add(_),w.classList.add(b)):"close"===b?(this.removeScrollEventAndStyleFromWidget(),w.removeAttribute("style"),w.classList.remove("h-open-notify"),w.classList.remove(_),w.classList.remove("expand")):(w.classList.add("h-open-notify"),w.classList.add(_))
break
case"resize_frame_notify":return!1
case"resize_frame":var y=document.getElementById(r.default.frameDivId),E=document.getElementsByTagName("BODY")[0],I=i&&i.open||"fc-open"
d.default.opened(h.isOpen),d.default.isOpened()?(this.removeScrollEventAndStyleFromWidget(),y.classList.add("h-open-container"),y.classList.add(I),s.default.publish("widget:opened"),E&&E.classList.add("fc-widget-open")):(y.removeAttribute("style"),y.classList.remove("h-open-container"),y.classList.remove(I),s.default.publish("widget:closed"),E&&E.classList.remove("fc-widget-open"))
break
case"toggle_frame":y=document.getElementById(r.default.frameDivId)
h.show?y.classList.remove("hide"):-1===y.className.trim().indexOf("hide")&&y.classList.add("hide")
break
case"push_frame_loaded":f.default.loaded(!0),f.default.setSource(e&&e.source),f.default.postMessage({action:"widget_location",openWindow:!1,payload:window.location.href}),a.default.postMessage({action:"widget_location",payload:{title:document.title,location:window.location.href}})
break
case"frame_state_change":var C=document.getElementById(r.default.frameDivId),M=h&&h.data,L=M&&M.frameState,k=a.default.getSettings()&&a.default.getSettings().enabledFeatures
s.default.publish("frame:statechange",h),"initialized"===L&&(a.default.isJWTEnabled()&&a.default.isJWTStrictMode()&&C.classList.add("fc_dn"),this.loadWidget(),k&&-1!==k.indexOf("COBROWSING")&&g.default.init({host:a.default.getHost(),locale:u.default.getProperties().locale})),"loaded"===L&&a.default.loaded(!0),"authenticated"!==L&&"not_authenticated"!==L||a.default.isJWTEnabled()&&a.default.isJWTStrictMode()&&(h.success?C.classList.remove("fc_dn"):304!==h.status&&this.unloadWidget())
break
case"widget_loaded":d.default.loaded(!0),s.default.publish("widget:loaded"),f.default.isLoaded()||f.default.init({host:h.pushDomain+"?ref="+btoa(window.location.origin),hostOrigin:o.default.url_domain(h.pushDomain),appLogoPath:h.appLogoPath}),d.default.doOpen()&&a.default.postMessage({action:"open_chat"}),a.default.postMessage({action:"widget_location",payload:{title:document.title,location:window.location.href}})
break
case"datastore_loaded":a.default.postMessage({action:"load_rules"})
break
case"rules_loaded":m.default.init(),p.default.start()
break
case"enlarge_image":c.default.create(h.picUrl)
break
case"expand_all":var O=i&&i.expanded||"expanded"
document.getElementById(r.default.frameDivId).classList.add("expanded-modal"),document.getElementById(r.default.frameDivId).classList.add(O)
break
case"collapse_all":O=i&&i.expanded||"expanded"
document.getElementById(r.default.frameDivId).classList.remove("expanded-modal"),document.getElementById(r.default.frameDivId).classList.remove(O)
break
case"user_state_change":s.default.publish("user:statechange",h)
break
case"user_authenticated":s.default.publish("user:authenticated",h)
break
case"user_created":var T=h.data
if(d.default.isLoaded())T&&u.default.setRestoreId(T.restoreId),this.onUserCreate(h)
else if(h.success){var P=T&&T.externalId,S=T&&T.restoreId,x=u.default.getExternalId()
x?x===P&&(u.default.setRestoreId(S),this.onUserCreate(h)):this.onUserCreate(h)}else this.onUserCreate(h)
break
case"user_cleared":s.default.publish("user:cleared"),f.default.postMessage({action:"clear:subscription",openWindow:!1}),u.default.reset()
break
case"message_sent":s.default.publish("message:sent",h)
break
case"message_received":s.default.publish("message:received",h)
break
case"ask_permission":f.default.postMessage({action:"ask:permission",openWindow:!0})
break
case"clear_subscription":f.default.postMessage({action:"clear:subscription",openWindow:!1})
break
case"unread_count_notify":s.default.publish("unreadCount:notify",h.data)
break
case"message_from_agent":s.default.publish("message:received",h.data)
break
case"message_from_user":s.default.publish("message:sent",h.data)
break
case"set_storage_item":if(o.default.storageAvailable){var j=h.data
j&&j.key&&j.value&&localStorage.setItem(j.key,JSON.stringify(j.value))}break
case"remove_storage_item":if(o.default.storageAvailable){var A=h.data
A&&A.key&&localStorage.removeItem(A.key)}break
case"add_reply_dialog_absolute_class_to_frame":d.default.isOpened()||this.addScrollEventAndStyleFromWidget()
break
case"remove_reply_dialog_absolute_class_to_frame":this.removeScrollEventAndStyleFromWidget()
break
case"startScreenShare":window.fc_cobrowse&&window.fc_cobrowse.allowScreenShare()
break
default:l.default.publish(v,h)}}},scrollHandlerForPositionCalculation:function(){var e=document.getElementById(r.default.frameDivId),t=document.getElementsByTagName("BODY")[0],n=screen.availHeight,i=.42
0!==window.orientation&&(n=screen.availWidth),n<=375?i=.51:n<=414?i=.29:n<=568?i=.365:n<=647?i=.468:n<=736&&(i=.486)
var o=t.scrollTop+n*i-e.offsetHeight
o+e.offsetHeight>=t.scrollHeight&&(o=t.scrollHeight-e.offsetHeight),e.style.top=Math.floor(o)+"px"},removeScrollEventAndStyleFromWidget:function(){var e=document.getElementById(r.default.frameDivId)
o.default.unbindEvent("scroll",this.scrollHandlerForPositionCalculation),e.classList.remove("fc_reply_position_absolute"),e.style.top="unset"},addScrollEventAndStyleFromWidget:function(){document.getElementById(r.default.frameDivId).classList.add("fc_reply_position_absolute"),o.default.bindEvent("scroll",this.scrollHandlerForPositionCalculation)},syncResponse:function(e,t){var n=e&&e.status
n?a.default.dispatch(t,{success:200===n,status:n}):a.default.dispatch(t,{success:!1,status:400})}}},{"../helpers/modal":1,"../helpers/omni":2,"../obj/constants":7,"../obj/state":8,"../obj/user":9,"./cb":10,"./common/ajax":11,"./common/dom":12,"./events":15,"./frame":16,"./msg_handler":19,"./msg_processor":20,"./push_frame":22}],18:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./msg_handler"),a=(i=o)&&i.__esModule?i:{default:i}
n.default={send:function(e,t,n){t&&"function"==typeof t&&(n=t,t=null),a.default.subscribe(e,t,n)}}},{"./msg_handler":19}],19:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=s(e("./frame")),o=s(e("./../obj/state")),a=s(e("./queue"))
function s(e){return e&&e.__esModule?e:{default:e}}n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e={}
return{subscribe:function(t,n,s){o.default.isLoaded()||i.default.isLoaded()&&-1!==i.default.loadingActions().indexOf("get_user_uuid")?(e[t+"_ack"]=s,i.default.postMessage({action:t,payload:n})):a.default.enque({action:t,payload:n,handler:s})},publish:function(t,n){i.default.dispatch(e[t],{success:n.success,status:n.status,data:n.data})}}}.bind(void 0)()},{"./../obj/state":8,"./frame":16,"./queue":23}],20:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=a(e("./queue")),o=a(e("./msg_handler"))
function a(e){return e&&e.__esModule?e:{default:e}}function s(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")}n.default={start:function(){i.default.isEmpty()||i.default.isLocked()||(i.default.toggleLock(!0),this.run())},run:function(){var e=i.default.deque()
e?this.process(e):i.default.toggleLock(!1)},process:function(e){var t=this
e.handler?o.default.subscribe(e.action,e.payload,function(n){s(this,t),"function"==typeof e.handler&&e.handler(n),this.run()}.bind(this)):(o.default.subscribe(e.action,e.payload),setTimeout(function(){s(this,t),this.run()}.bind(this),0))}}},{"./msg_handler":19,"./queue":23}],21:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./msg_handler"),a=(i=o)&&i.__esModule?i:{default:i}
n.default={send:function(e,t){return new Promise(function(n,i){a.default.subscribe(e,t,function(e){var t=e&&e.status,o=e&&e.success
200===t||o?n(e):i(e)})})}}},{"./msg_handler":19}],22:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=a(e("./common/dom")),o=a(e("../obj/constants"))
function a(e){return e&&e.__esModule?e:{default:e}}n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=null,t=null,n=null,a=!1,s=null
return{isLoaded:function(){return a},loaded:function(e){a=e},getSource:function(){return s},setSource:function(e){s=e},getHost:function(){return e},getHostOrigin:function(){return t},getLogoPath:function(){return n},init:function(i){e=i.host,t=i.hostOrigin,n=i.appLogoPath,this.load()},reset:function(){e=null,t=null,n=null,a=!1,s=null},postMessage:function(t){t.openWindow?window.open(e+"&action="+btoa(t.action)+"&appLogoPath="+btoa(n),"_blank","toolbar=yes,scrollbars=yes,resizable=yes,top=500,left=500,width=400,height=400"):a&&this.post({action:t.action,payload:t.payload})},post:function(e){s.postMessage(e,t)},add:function(){var t=document,n=t.body,a=t.getElementById(o.default.pushFrameDivId),s=t.createElement("IFRAME")
a&&"DIV"===a.tagName||(a=t.createElement("DIV"),n.appendChild(a)),a.setAttribute("id",o.default.pushFrameDivId),i.default.setAttr(s,{id:o.default.pushFrameId,src:e,frameborder:"0"}),a.appendChild(s)},load:function(){i.default.isPushSupportedByBrowser()&&this.add()},unload:function(){i.default.remove(o.default.pushFrameDivId),this.reset()}}}.bind(void 0)()},{"../obj/constants":7,"./common/dom":12}],23:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i,o=e("./msg_handler");(i=o)&&i.__esModule
n.default=function(){(function(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")})(void 0,void 0)
var e=[],t=!1
return{enque:function(n){t||(void 0===e&&(e=[]),e.push(n))},deque:function(){if(e&&e.length)return e.shift()},toggleLock:function(e){t=e},isLocked:function(){return t},isEmpty:function(){return!e||!e.length}}}.bind(void 0)()},{"./msg_handler":19}],24:[function(e,t,n){"use strict"
Object.defineProperty(n,"__esModule",{value:!0})
var i=v(e("./obj/constants")),o=v(e("./obj/state")),a=v(e("./obj/user")),s=v(e("./utils/msg_promise")),r=v(e("./utils/msg_callback")),d=v(e("./utils/msg_handler")),u=v(e("./utils/common/dom")),l=v(e("./utils/frame")),f=v(e("./utils/message")),c=v(e("./utils/events")),m=(v(e("./utils/common/ajax")),v(e("./utils/push_frame"))),p=v(e("./helpers/omni/analytics")),g=v(e("./helpers/omni/transition")),h=v(e("./helpers/omni"))
function v(e){return e&&e.__esModule?e:{default:e}}function b(e,t){if(e!==t)throw new TypeError("Cannot instantiate an arrow function")}n.default=function(){b(void 0,void 0)
var e=function(){var e=document,t=e.createElement("link"),n=u.default.cdn_url()?u.default.cdn_url():l.default.getHost()
t.href=n+"/css/widget.css?t="+(new Date).getTime(),t.rel="stylesheet",e.getElementsByTagName("head")[0].appendChild(t)}
return{init:function(t){var n,i=-1!==["Chrome","Firefox","Safari","Edge","Trident","WebView"].indexOf(u.default.getAgent.name),s=-1!==["Station"].indexOf(u.default.getAgent.appName)
i&&!s&&(t&&t.token&&(window.location.origin||(window.location.origin=window.location.protocol+"//"+window.location.hostname+(window.location.port?":"+window.location.port:"")),t.referrer=btoa(window.location.origin),void 0===t.open&&(t.open=!1),n=t,o.default.reset(),a.default.reset(),h.default.destroy(),f.default.unsubscribe(),l.default.init({host:n.host.trim(),token:n.token.trim(),referrer:n.referrer,siteId:n.siteId,jwtAuthToken:n.jwtAuthToken}),o.default.openOnLoad(n.open),n.identifyByReferenceId&&a.default.setIdentifyByReferenceId(!0),n.externalId&&(a.default.setExternalId(n.externalId),a.default.setRestoreId(n.restoreId)),n.tags&&n.tags.length&&o.default.setTags(n.tags),n.faqTags&&o.default.setFaqTags(n.faqTags),a.default.setProperties({firstName:n.firstName,lastName:n.lastName,email:n.email,phone:n.phone,phoneCountry:n.phoneCountryCode,locale:n.locale,meta:n.meta}),a.default.setConfig(n.config),n.onLoad&&c.default.subscribe("widget:loaded",n.onLoad),l.default.add(),f.default.subscribe(),e()))},destroy:function(){m.default.unload(),l.default.unload()},isOpen:function(){return o.default.isOpened()},isInitialized:function(){return!!l.default.getToken()},isLoaded:function(){return o.default.isLoaded()},on:function(e,t){c.default.subscribe(e,t)},off:function(e,t){c.default.unsubscribe(e,t)},open:function(e){e?d.default.subscribe("open_channel",e):d.default.subscribe("open_chat")},close:function(){d.default.subscribe("close_chat")},show:function(){d.default.subscribe("show_chat")},hide:function(){d.default.subscribe("hide_chat")},setTags:function(e){o.default.isLoaded()&&(e&&e.length?o.default.setTags(e):o.default.setTags([]),d.default.subscribe("set_tags",{tags:o.default.getTags(),force:!0}))},setFaqTags:function(e){o.default.isLoaded()&&(e?o.default.setFaqTags(e):o.default.setFaqTags({}),d.default.subscribe("set_faq_tags",{faqTags:o.default.getFaqTags(),force:!0}))},setExternalId:function(e,t){var n={}
if(e){if("string"==typeof e?n.externalId=e:n=e,!(void 0===window.Promise||t&&"function"==typeof t))return s.default.send("set_external_id",n)
r.default.send("set_external_id",n,t)}},setConfig:function(e){var t=document.getElementById(i.default.frameDivId)
t&&e&&(void 0!==e.fullscreen&&(t.classList.remove(i.default.classes.fullscreenClass),!0===e.fullscreen&&t.classList.add(i.default.classes.fullscreenClass)),e.headerProperty&&(void 0!==e.headerProperty.direction&&(t.classList.remove("fc_l2r"),"ltr"===e.headerProperty.direction&&t.classList.add("fc_l2r")),void 0!==e.headerProperty.hideChatButton&&(t.classList.remove("fc_dn"),e.headerProperty.hideChatButton&&t.classList.add("fc_dn")))),d.default.subscribe("set_custom_config",e)},user:{get:function(e){if(!(void 0===window.Promise||e&&"function"==typeof e))return s.default.send("get_user")
r.default.send("get_user",e)},isExists:function(e){if(!(void 0===window.Promise||e&&"function"==typeof e))return s.default.send("user_exists")
r.default.send("user_exists",e)},update:function(e,t){if(e.jwtAuthToken?l.default.setJWTAuthToken(e.jwtAuthToken):(e.firstName&&a.default.setFirstName(e.firstName),e.lastName&&a.default.setLastName(e.lastName),e.email&&a.default.setEmail(e.email),e.phone&&a.default.setPhone(e.phone),e.phoneCountry&&a.default.setPhoneCountry(e.phoneCountry),e.meta&&a.default.setUserMeta(e.meta)),!(void 0===window.Promise||t&&"function"==typeof t))return s.default.send("update_user",e)
r.default.send("update_user",e,t)},setProperties:function(e,t){var n={}
if(e.jwtAuthToken?(l.default.setJWTAuthToken(e.jwtAuthToken),n=e):(e.firstName&&(n.firstName=e.firstName,e.firstName=void 0),e.lastName&&(n.lastName=e.lastName,e.lastName=void 0),e.email&&(n.email=e.email,e.email=void 0),e.phone&&(n.phone=e.phone,e.phone=void 0),e.phoneCountryCode&&(n.phoneCountry=e.phoneCountryCode,e.phoneCountryCode=void 0),e.locale&&(n.locale=e.locale,e.locale=void 0),n.meta=e,a.default.setProperties(n)),!(void 0===window.Promise||t&&"function"==typeof t))return s.default.send("set_user_properties",n)
r.default.send("set_user_properties",n,t)},setFirstName:function(e,t){var n={}
if(e){if("string"==typeof e?n.firstName=e:n=e,!t)return this.setProperties(n)
this.setProperties(n,t)}},setLastName:function(e,t){var n={}
if(e){if("string"==typeof e?n.lastName=e:n=e,!t)return this.setProperties(n)
this.setProperties(n,t)}},setEmail:function(e,t){var n={}
if(e){if("string"==typeof e?n.email=e:n=e,!t)return this.setProperties(n)
this.setProperties(n,t)}},setPhone:function(e,t){var n={}
if(e){if("string"==typeof e?n.phone=e:n=e,!t)return this.setProperties(n)
this.setProperties(n,t)}},setPhoneCountryCode:function(e,t){var n={}
if(e){if("string"==typeof e?n.phoneCountryCode=e:n=e,!t)return this.setProperties(n)
this.setProperties(n,t)}},setMeta:function(e,t){if(!t)return this.setProperties(e)
this.setProperties(e,t)},setLocale:function(e,t){var n={}
if(e){if("string"==typeof e?n.locale=e:n=e,!t)return this.setProperties(n)
this.setProperties(n,t)}},clear:function(e){if(!(void 0===window.Promise||e&&"function"==typeof e))return s.default.send("reset_user")
r.default.send("reset_user",e)},create:function(e,t){if("function"==typeof e&&(t=e,e=null),!(void 0===window.Promise||t&&"function"==typeof t))return s.default.send("create_user",e)
r.default.send("create_user",e,t)},getUUID:function(e){if(!(void 0===window.Promise||e&&"function"==typeof e))return s.default.send("get_user_uuid")
r.default.send("get_user_uuid",e)},generateUUID:function(e){if(!(void 0===window.Promise||e&&"function"==typeof e))return s.default.send("generate_user_uuid")
r.default.send("generate_user_uuid",e)}},track:p.default.track,trackPage:function(e,t){g.default.track({newValue:e,title:t,force:!0})},authenticate:function(e){var t=this
if(l.default.isJWTEnabled()){var n=function(){b(this,t),l.default.setJWTAuthToken(e),l.default.postMessage({action:"authenticate_user",payload:e})}.bind(this),i=function(i){b(this,t),i&&200===i.status?this.user.update({jwtAuthToken:e}):n()}.bind(this)
this.isLoaded()?void 0===window.Promise?this.user.get(i):this.user.get().then(i,i):n()}}}}.bind(void 0)()},{"./helpers/omni":2,"./helpers/omni/analytics":4,"./helpers/omni/transition":5,"./obj/constants":7,"./obj/state":8,"./obj/user":9,"./utils/common/ajax":11,"./utils/common/dom":12,"./utils/events":15,"./utils/frame":16,"./utils/message":17,"./utils/msg_callback":18,"./utils/msg_handler":19,"./utils/msg_promise":21,"./utils/push_frame":22}],25:[function(e,t,n){n.config={type:"production",cdn:{enabled:{forAssets:!1,forApi:!1},assets:"assetscdn-",api:"apicdn-",domain:{production:"wchat.freshchat.com",staging:"webchat-msgreen.freshpori.com",local:"localhost:8580"},subDomain:"",protocol:{local:"http://",production:"https://",staging:"https://"}}}},{}]},{},[6])
